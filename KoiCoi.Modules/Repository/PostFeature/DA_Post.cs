
using KoiCoi.Models;
using Microsoft.Extensions.Configuration;
using Serilog;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace KoiCoi.Modules.Repository.PostFeature;

public class DA_Post
{
    private readonly AppDbContext _db;
    private readonly SaveNotifications _saveNotifications;
    private readonly IConfiguration _configuration;

    public DA_Post(AppDbContext db, IConfiguration configuration,SaveNotifications saveNotifications)
    {
        _db = db;
        _configuration = configuration;
        _saveNotifications = saveNotifications;
    }

    public async Task<Result<string>> CreatePostFeature(CreatePostPayload payload, int LoginUserId)
    {
        Result<string> result = null;
        try
        {
            string balanceSalt = _configuration["appSettings:BalanceSalt"] ?? throw new Exception("Invalid Balance Salt");
            int EventId = Convert.ToInt32(Encryption.DecryptID(payload.EventIdval!, LoginUserId.ToString()));
            int? TagId = payload.TagIdval is not null ? Convert.ToInt32(Encryption.DecryptID(payload.TagIdval, LoginUserId.ToString())) : null;
            
            PostPolicyPropertyPayload viewPolicy= payload.policyProperties[0];

            Post newPost = new Post
            {
                Content = payload.Content,
                EventId = EventId,
                TagId = TagId,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow,
                Inactive = false,
            };
            await _db.Posts.AddAsync(newPost);
            await _db.SaveChangesAsync();
            int policyId = 1;
            foreach (var policy in payload.policyProperties)
            {
                PostPolicyProperty newPostPolicy = new PostPolicyProperty
                {
                    PostId =newPost.PostId,
                    PolicyId = policyId,
                    MaxCount = policy.MaxCount,
                    StartDate = policy.StartDate,
                    EndDate = policy.EndDate,
                    GroupMemberOnly = policy.GroupMemberOnly,
                    FriendOnly = policy.FriendOnly
                };
                policyId++;
            }
            var checkEventOwner = await (from _em in _db.EventMemberships
                                         join _ust in _db.UserTypes on _em.UserTypeId equals _ust.TypeId
                                         where _em.EventId == EventId
                                         && _em.UserId == LoginUserId
                                         && _ust.Name.ToLower() == "owner"
                                         select new
                                         {
                                             LoginId = _em.UserId
                                         })
                                         .FirstOrDefaultAsync();
            ///Save Post Images
            string baseDirectory = _configuration["appSettings:UploadPath"] ?? throw new Exception("Invalid UploadPath");
            string uploadDirectory = _configuration["appSettings:PostImages"] ?? throw new Exception("Invalid function upload path.");
            string destDirectory = Path.Combine(baseDirectory, uploadDirectory);
            if (!Directory.Exists(destDirectory))
            {
                Directory.CreateDirectory(destDirectory);
            }
            foreach (var item in payload.imageData)
            {
                DateTime now = DateTime.UtcNow;
                string filename = $"{now.ToString("fffss_")}" + Guid.NewGuid().ToString("N").Substring(0, 8) + $"{now.ToString("-HHmm")}" + ".png";
                string base64Str = item.imagebase64!;
                byte[] bytes = Convert.FromBase64String(base64Str!);

                string filePath = Path.Combine(destDirectory, filename);
                if (filePath.Contains(".."))
                { //if found .. in the file name or path
                    Log.Error("Invalid path " + filePath);
                }
                await System.IO.File.WriteAllBytesAsync(filePath, bytes);

                var newImage = new PostImage
                {
                    Url = filename,
                    Description = item.description,
                    PostId = newPost.PostId,
                    CreatedDate = DateTime.UtcNow,
                };
                await _db.PostImages.AddAsync(newImage);
                await _db.SaveChangesAsync();
            }
            if (checkEventOwner is not null)
            {
                ///Already Approved because Post Creator is event owner
                int approvedStatus = await _db.StatusTypes
                    .Where(x=> x.StatusName.ToLower() == "approved")
                    .Select(x=> x.StatusId).FirstOrDefaultAsync();
                CollectPost newCollect = new CollectPost
                {
                    PostId = newPost.PostId,
                    CollectAmount = Encryption.EncryptID(payload.CollectAmount!.ToString()!, balanceSalt),
                    CreatorId = LoginUserId,
                    StatusId = approvedStatus,
                };
                await _db.CollectPosts.AddAsync(newCollect);
                await _db.SaveChangesAsync();

                ///Update Event TotalBalance Amount and LastBalance Amount
                Event? parentEvent = await _db.Events
                    .Where(x=> x.Eventid == EventId)
                    .FirstOrDefaultAsync();
                if (parentEvent is not null)
                {
                    decimal EventTotalBalance = Globalfunction.StringToDecimal(
                        parentEvent.TotalBalance == "0" || parentEvent.TotalBalance == null ? "0" :
                        Encryption.DecryptID(parentEvent.TotalBalance.ToString(), balanceSalt));
                    decimal EventLastBalance = Globalfunction.StringToDecimal(
                            parentEvent.LastBalance == "0" || parentEvent.LastBalance == null ? "0" :
                            Encryption.DecryptID(parentEvent.LastBalance.ToString(), balanceSalt));
                    EventTotalBalance = EventTotalBalance + payload.CollectAmount;
                    EventLastBalance = EventLastBalance + payload.CollectAmount;
                    parentEvent.TotalBalance = Encryption.EncryptID(EventTotalBalance.ToString(), balanceSalt);
                    parentEvent.LastBalance = Encryption.EncryptID(EventLastBalance.ToString(), balanceSalt);
                    await _db.SaveChangesAsync();
                }

                ///Update Channel TotalBalance Amount and LastBalance Amount
                Channel? parentChannel = await (from _chan in _db.Channels
                                                join _ev in _db.Events on _chan.ChannelId equals _ev.ChannelId
                                                where _ev.Eventid == EventId
                                                select _chan)
                                               .FirstOrDefaultAsync();
                if(parentChannel is not null)
                {
                    decimal ChannelTotalBalance = Globalfunction.StringToDecimal(
                        parentChannel.TotalBalance == "0" || parentChannel.TotalBalance == null ? "0" :
                        Encryption.DecryptID(parentChannel.TotalBalance.ToString(), balanceSalt));
                    decimal ChannelLastBalance = Globalfunction.StringToDecimal(
                            parentChannel.LastBalance == "0" || parentChannel.LastBalance == null ? "0" :
                            Encryption.DecryptID(parentChannel.LastBalance.ToString(), balanceSalt));
                    ChannelTotalBalance = ChannelTotalBalance + payload.CollectAmount;
                    ChannelLastBalance = ChannelLastBalance + payload.CollectAmount;
                    parentChannel.TotalBalance = Encryption.EncryptID(ChannelTotalBalance.ToString(), balanceSalt);
                    parentChannel.LastBalance = Encryption.EncryptID(ChannelLastBalance.ToString(), balanceSalt);
                    await _db.SaveChangesAsync();
                }

                ///Notifi the members if post privicy is not private
                if (viewPolicy.MaxCount == 0)/// maxcount(0) mean private
                {
                    List<int> channelMembers = await (from _ev in _db.Events
                                                      join _chan in _db.Channels on _ev.ChannelId equals _chan.ChannelId
                                                      join _chme in _db.ChannelMemberships on _chan.ChannelId equals _chme.ChannelId
                                                      where _ev.Eventid == EventId
                                                      select _chme.UserId).ToListAsync();
                    if (channelMembers.Contains(LoginUserId))
                    {
                        channelMembers.Remove(LoginUserId);
                    }
                    channelMembers.Distinct();
                    string? LoginName = await _db.Users.Where(x => x.UserId == LoginUserId)
                        .Select(x => x.Name).FirstOrDefaultAsync();
                    await _saveNotifications.SaveNotification(
                        channelMembers, LoginUserId,
                        $"New Post in {parentEvent?.EventName}",
                        $"{LoginName} Collected {payload.CollectAmount} in {parentEvent?.EventName}",
                        $"NewCollectPostAdded/{newPost.PostId}");

                    ///Notifi to Post Uploader that upload success
                    await _saveNotifications.SaveNotification(
                            new List<int> { LoginUserId },
                            LoginUserId,
                            $"Upload Posting Success",
                            $"Tap to See you Details ",
                            $"NewCollectPostAdded/{newPost.PostId}"
                            );
                }
            }
            else
            {
                ///Pending post to approve by a admin
                int pendingStatus = await _db.StatusTypes
                    .Where(x => x.StatusName.ToLower() == "pending")
                    .Select(x => x.StatusId).FirstOrDefaultAsync();
                CollectPost newCollect = new CollectPost
                {
                    PostId = newPost.PostId,
                    CollectAmount = Encryption.EncryptID(payload.CollectAmount!.ToString()!, balanceSalt),
                    CreatorId = LoginUserId,
                    StatusId = pendingStatus,
                };
                await _db.CollectPosts.AddAsync(newCollect);
                await _db.SaveChangesAsync();


                ///Notifie to event admins{Note: don't Channel Admins}
                List<int> admins = await (from eme in _db.EventMemberships
                                          join _ut in _db.UserTypes on eme.UserTypeId equals _ut.TypeId
                                          where eme.EventId == EventId
                                          && (_ut.Name.ToLower() == "admin" || _ut.Name.ToLower() == "owner")
                                          select eme.UserId).ToListAsync();
                if(admins.Contains(LoginUserId))
                {
                    admins.Remove(LoginUserId);
                }
                admins.Distinct();
                Event? parentEvent = await _db.Events
                    .Where(x => x.Eventid == EventId)
                    .FirstOrDefaultAsync();
                string? LoginName = await _db.Users.Where(x => x.UserId == LoginUserId)
                        .Select(x => x.Name).FirstOrDefaultAsync();
                await _saveNotifications.SaveNotification(
                    admins,
                    LoginUserId,
                    $"Requested to Collect Posts in {parentEvent?.EventName}",
                    $"{LoginName} Collected {payload.CollectAmount} in {parentEvent?.EventName}",
                    $"RequestedNewCollectPost/{newPost.PostId}"
                    );

                ///Notifi to Post Uploader that upload success
                await _saveNotifications.SaveNotification(
                        new List<int> { LoginUserId },
                        LoginUserId,
                        $"Upload Posting Success",
                        $"Tap to See you Details ",
                        $"RequestedNewCollectPost/{newPost.PostId}"
                        );
            }
            result = Result<string>.Success("Posting Success");
        }
        catch (Exception ex)
        {
            result = Result<string>.Error(ex);
        }
        return result;
    }

    public async Task<Result<string>> CreatePostTags(CreatePostTagListPayload payload, int LoginUserId)
    {
        Result<string> result = null;
        try
        {
            int EventId = Convert.ToInt32(Encryption.DecryptID(payload.EventIdval!, LoginUserId.ToString()));
            var checkChannelMember = await (from _chan in _db.Channels
                                            join _event in _db.Events on _chan.ChannelId equals _event.ChannelId
                                            join _meme in _db.ChannelMemberships on _chan.ChannelId equals _meme.ChannelId
                                            where _event.Eventid == EventId && _meme.UserId == LoginUserId
                                            select _meme).FirstOrDefaultAsync();
            if (checkChannelMember == null) return Result<string>.Error("Channel Member Only Can create PostTags");
            List<PostTagPayload> postTags = payload.PostTags;
            foreach (var item in postTags)
            {
                PostTag newTag = new PostTag
                {
                    TagName = item.PostTagName,
                    TagDescription = item.PostTagDescritpion,
                    EventId = EventId,
                    CreatorId = LoginUserId,
                    CreateDate = DateTime.UtcNow,
                    Inactive = false
                };
                await _db.PostTags.AddAsync(newTag);
                await _db.SaveChangesAsync();
            }
            result = Result<string>.Success("Success");
        }
        catch(Exception ex)
        {
            result = Result<string>.Error(ex);
        }
        return result;
    }


    public async Task<Result<List<PostTagDataResponse>>> GetPostTags(string EventIdval, int LoginUserId)
    {
        Result<List<PostTagDataResponse>> result;
        try
        {
            int EventId = Convert.ToInt32(Encryption.DecryptID(EventIdval, LoginUserId.ToString()));
            List<PostTagDataResponse> query = await _db.PostTags
                .Where(x => x.EventId == EventId)
                .Select(x => new PostTagDataResponse
                {
                    PostTagIdval = Encryption.EncryptID(x.TagId.ToString(), LoginUserId.ToString()),
                    PostTagName = x.TagName,
                    PostTagDescritpion = x.TagDescription
                }).ToListAsync();
            result = Result<List<PostTagDataResponse>>.Success(query);
        }
        catch (Exception ex)
        {
            result = Result<List<PostTagDataResponse>>.Error(ex);
        }

        return result;
    }

    public async Task<Result<List<ReactTypeResponse>>> GetAllReactType(int LoginUserId)
    {
        Result<List<ReactTypeResponse>> result = null;
        try
        {
            List<ReactTypeResponse> querylist = await _db.ReactTypes.Select(
                x => new ReactTypeResponse {
                    TypeIdval = Encryption.EncryptID(x.TypeId.ToString(),LoginUserId.ToString()),
                    Emoji = x.Emoji,
                    Description = x.Description
            }).ToListAsync();
            result = Result<List<ReactTypeResponse>>.Success(querylist);
        }
        catch (Exception ex)
        {
            result = Result<List<ReactTypeResponse>>.Error(ex);
        }
        return result;
    }
}
