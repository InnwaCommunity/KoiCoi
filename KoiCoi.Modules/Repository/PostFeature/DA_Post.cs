
using KoiCoi.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace KoiCoi.Modules.Repository.PostFeature;

public class DA_Post
{
    private readonly AppDbContext _db;
    private readonly NotificationManager.NotificationManager _notificationmanager;
    private readonly IConfiguration _configuration;

    public DA_Post(AppDbContext db, IConfiguration configuration, NotificationManager.NotificationManager notificationmanager)
    {
        _db = db;
        _configuration = configuration;
        _notificationmanager = notificationmanager;
    }

    public async Task<Result<string>> CreatePostFeature(CreatePostPayload payload, int LoginUserId)
    {
        Result<string> result = null;
        try
        {
            string balanceSalt = _configuration["appSettings:BalanceSalt"] ?? throw new Exception("Invalid Balance Salt");
            int EventId = Convert.ToInt32(Encryption.DecryptID(payload.EventIdval!, LoginUserId.ToString()));
            int? TagId = payload.TagIdval is not null ? Convert.ToInt32(Encryption.DecryptID(payload.TagIdval, LoginUserId.ToString())) : null;


            ///Check EventId EndDate
            DateTime? eventEndDate = _db.Events.Where(x => x.Eventid == EventId)
                .Select(x => x.EndDate).FirstOrDefault();
            if(eventEndDate == null || eventEndDate < DateTime.UtcNow)
            {
                ///Notifi to Post Uploader that upload success
                await _notificationmanager.SaveNotification(
                        new List<int> { LoginUserId },
                        LoginUserId,
                        $"Upload Fail",
                        $"Post Upload fail because Event Ended",
                        $"NewCollectPostAdded/null"
                        );
                return  Result<string>.Error("Can Not Add");
            }
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
                await _db.PostPolicyProperties.AddAsync(newPostPolicy);
                await _db.SaveChangesAsync();
                policyId++;
            }
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
                string filename = Globalfunction.NewUniqueFileName() + ".png";
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
                    await _notificationmanager.SaveNotification(
                        channelMembers, LoginUserId,
                        $"New Post in {parentEvent?.EventName}",
                        $"{LoginName} Collected {payload.CollectAmount} in {parentEvent?.EventName}",
                        $"NewCollectPostAdded/{newPost.PostId}");

                    ///Notifi to Post Uploader that upload success
                    await _notificationmanager.SaveNotification(
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
                await _notificationmanager.SaveNotification(
                    admins,
                    LoginUserId,
                    $"Requested to Collect Posts in {parentEvent?.EventName}",
                    $"{LoginName} Collected {payload.CollectAmount} in {parentEvent?.EventName}",
                    $"RequestedNewCollectPost/{newPost.PostId}"
                    );

                ///Notifi to Post Uploader that upload success
                await _notificationmanager.SaveNotification(
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
    public async Task<Result<List<ReviewPostResponse>>> ReviewPostsList(string EventIdval,string StatusName, int LoginUserId)
    {
        Result<List<ReviewPostResponse>> result = null;
        try
        {
            string balanceSalt = _configuration["appSettings:BalanceSalt"] ?? throw new Exception("Invalid Balance Salt");
            int EventId = Convert.ToInt32(Encryption.DecryptID(EventIdval, LoginUserId.ToString()));
            List<ReviewPostResponse> query = await (from _event in _db.Events
                                                            join _post in _db.Posts on _event.Eventid equals _post.EventId
                                                            join _cp in _db.CollectPosts on _post.PostId equals _cp.PostId
                                                            join _creator in _db.Users on _cp.CreatorId equals _creator.UserId
                                                            join _status in _db.StatusTypes on _cp.StatusId equals _status.StatusId
                                                            join _em in _db.EventMemberships on _event.Eventid equals _em.EventId
                                                            join _logu in _db.Users on _em.UserId equals _logu.UserId
                                                            join _ut in _db.UserTypes on _em.UserTypeId equals _ut.TypeId
                                                            where _event.Eventid == EventId &&
                                                            _status.StatusName.ToLower() == StatusName && 
                                                            _logu.UserId == LoginUserId &&
                                                            (_ut.Name.ToLower() == "admin" ||
                                                            _ut.Name.ToLower() == "owner")
                                                            select new ReviewPostResponse
                                                            {
                                                                PostIdval = Encryption.EncryptID(_post.PostId.ToString(),LoginUserId.ToString()),
                                                                Content = _post.Content ?? "",
                                                                TagIdval = _post.TagId != null ? Encryption.EncryptID(_post.TagId!.Value.ToString(), LoginUserId.ToString()) : "",
                                                                TagName = _post.TagId != null ? _db.PostTags.Where(x=> x.TagId == _post.TagId!).Select(x=> x.TagName).FirstOrDefault() : "",
                                                                CreatorIdval = Encryption.EncryptID(_creator.UserId.ToString(), LoginUserId.ToString()),
                                                                CreatorName = _creator.Name,
                                                                CollectAmount = Globalfunction.StringToDecimal(Encryption.DecryptID(_cp.CollectAmount,balanceSalt)),
                                                                CreatedDate = _post.CreatedDate,
                                                                ImageResponse = _db.PostImages.Where(x => x.PostId == _post.PostId)
                                                                                            .Select(x => new PostImageResponse
                                                                                            {
                                                                                                ImageIdval = Encryption.EncryptID(x.ImageId.ToString(), LoginUserId.ToString()),
                                                                                                ImageUrl = x.Url,
                                                                                                Description = x.Description
                                                                                             }).ToList()
                                                            }).ToListAsync();
            result= Result<List<ReviewPostResponse>>.Success(query);
        }
        catch (Exception ex)
        {
            result = Result<List<ReviewPostResponse>>.Error(ex);
        }
        return result;
    }


    public async Task<Result<string>> ApproveOrRejectPost(List<ApproveRejectPostPayload> payload, int LoginUserId)
    {
        Result<string> result = null;
        try
        {
            string balanceSalt = _configuration["appSettings:BalanceSalt"] ?? throw new Exception("Invalid Balance Salt");
            foreach (var item in payload)
            {
                int PostId = Convert.ToInt32(Encryption.DecryptID(item.PostIdval!, LoginUserId.ToString()));
                var collectPost = await _db.CollectPosts.Where(x=> x.PostId == PostId).FirstOrDefaultAsync();
                if(collectPost is not null && collectPost.StatusId == 1)
                {
                    if(item.AppRejStatus == 1)///Approve
                    {
                        int approvedId = _db.StatusTypes.Where(x=> x.StatusName == "approved").
                            Select(x=> x.StatusId).FirstOrDefault();
                        collectPost.StatusId = approvedId;
                        collectPost.ApproverId = LoginUserId;
                        await _db.SaveChangesAsync();
                        ///add amount to event and channel amount
                        decimal collectAmount = Globalfunction.StringToDecimal(
                                                            Encryption.DecryptID(collectPost.CollectAmount, balanceSalt));
                        var eventdata = await (from _post in _db.Posts
                                               join _eve in _db.Events on _post.EventId equals _eve.Eventid
                                               where _post.PostId == PostId
                                               select _eve).FirstOrDefaultAsync();
                        if(eventdata is not null)
                        {
                            decimal oldTotalAmount = Globalfunction.StringToDecimal(
                                eventdata.TotalBalance == "0" || eventdata.TotalBalance == null ? "0" :
                                                            Encryption.DecryptID(eventdata.TotalBalance, balanceSalt));
                            decimal oldLastAmount = Globalfunction.StringToDecimal(
                                eventdata.LastBalance == "0" || eventdata.LastBalance == null ? "0" :
                                                            Encryption.DecryptID(eventdata.LastBalance, balanceSalt));
                            decimal newTotalAmount = oldTotalAmount + collectAmount;
                            decimal newLastAmount = oldLastAmount + collectAmount;
                            eventdata.TotalBalance = Encryption.EncryptID(newTotalAmount.ToString(), balanceSalt);
                            eventdata.LastBalance = Encryption.EncryptID(newLastAmount.ToString(), balanceSalt);
                            await _db.SaveChangesAsync();
                        }
                        var channelData = await (from _post in _db.Posts
                                               join _eve in _db.Events on _post.EventId equals _eve.Eventid
                                               join _chn in _db.Channels on _eve.ChannelId equals _chn.ChannelId
                                               where _post.PostId == PostId
                                               select _chn).FirstOrDefaultAsync();
                        if (channelData is not null)
                        {
                            decimal oldTotalAmount = Globalfunction.StringToDecimal(
                                channelData.TotalBalance == "0" || channelData.TotalBalance == null ? "0" :
                                                            Encryption.DecryptID(channelData.TotalBalance, balanceSalt));
                            decimal oldLastAmount = Globalfunction.StringToDecimal(
                                channelData.LastBalance == "0" || channelData.LastBalance == null ? "0" :
                                                            Encryption.DecryptID(channelData.LastBalance, balanceSalt));
                            decimal newTotalAmount = oldTotalAmount + collectAmount;
                            decimal newLastAmount = oldLastAmount + collectAmount;
                            channelData.TotalBalance = Encryption.EncryptID(newTotalAmount.ToString(), balanceSalt);
                            channelData.LastBalance = Encryption.EncryptID(newLastAmount.ToString(), balanceSalt);
                            await _db.SaveChangesAsync();
                        }


                    }
                    if(item.AppRejStatus == 2)///Reject
                    {
                        int rejectId = _db.StatusTypes.Where(x => x.StatusName == "rejected").
                            Select(x => x.StatusId).FirstOrDefault();
                        collectPost.StatusId = rejectId;
                        collectPost.ApproverId = LoginUserId;
                        await _db.SaveChangesAsync();
                    }
                    var notinfo = await (from _col in _db.CollectPosts
                                         join _creator in _db.Users on _col.CreatorId equals _creator.UserId
                                         join _approver in _db.Users on _col.ApproverId equals _approver.UserId
                                         where _col.PostId == PostId
                                         select new
                                         {
                                             CreatorId = _creator.UserId,
                                             CreatorName = _creator.Name,
                                             ApproverId = _approver.UserId,
                                             ApproverName = _approver.Name
                                         }).FirstOrDefaultAsync();
                    if(notinfo is not null)
                    {
                        string paction = "";
                        if (item.AppRejStatus == 1)
                        {
                            paction = "Approved";
                        }
                        else if (item.AppRejStatus == 2)
                        {
                            paction = "Rejected";
                        }
                        await _notificationmanager.SaveNotification(
                            new List<int> { LoginUserId },
                            LoginUserId,
                            $"{paction} your post by {notinfo.ApproverName}",
                            $"{paction} your post by {notinfo.ApproverName}",
                            $"ApprovedOrRejectedPosts/{PostId}"
                            );
                    }
                }
            }
            result = Result<string>.Success("Success");
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
