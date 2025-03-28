﻿using KoiCoi.Database.AppDbContextModels;
using KoiCoi.Models.EventDto.Payload;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Drawing.Printing;
using System.Management;
using System.Threading.Channels;

namespace KoiCoi.Modules.Repository.PostFeature;

public class DA_Post
{
    private readonly AppDbContext _db;
    private readonly NotificationManager.NotificationManager _notificationmanager;
    private readonly IConfiguration _configuration;
    private readonly KcAwsS3Service _kcAwsS3Service;

    public DA_Post(
        AppDbContext db, 
        IConfiguration configuration, 
        NotificationManager.NotificationManager notificationmanager,
        KcAwsS3Service kcAwsS3Service)
    {
        _db = db;
        _configuration = configuration;
        _notificationmanager = notificationmanager;
        _kcAwsS3Service = kcAwsS3Service;
    }

    public async Task<Result<string>> CreatePostFeature(CreatePostPayload payload, int LoginUserId)
    {
        Result<string> result = null;
        try
        {
            string balanceSalt = _configuration["appSettings:BalanceSalt"] ?? throw new Exception("Invalid Balance Salt");
            int EventPostId = Convert.ToInt32(Encryption.DecryptID(payload.EventPostIdval!, LoginUserId.ToString()));
            int CreatorId = LoginUserId;
            if(payload.CreatorIdval is not null)
            {
                CreatorId= Convert.ToInt32(Encryption.DecryptID(payload.CreatorIdval, LoginUserId.ToString()));
            }
            //int? TagId = payload.TagIdval is not null ? Convert.ToInt32(Encryption.DecryptID(payload.TagIdval, LoginUserId.ToString())) : null;

            //int MarkId = Convert.ToInt32(Encryption.DecryptID(payload.MarkIdval!,LoginUserId.ToString()));

            ///Check EventId EndDate
            DateTime? eventEndDate = _db.Events.Where(x => x.PostId == EventPostId)
                .Select(x => x.EndDate).FirstOrDefault();
            if (eventEndDate == null || eventEndDate < DateTime.UtcNow)
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

            Post newPost = new Post
            {
                PostType="collectPost",
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow,
                Inactive = false,
            };
            await _db.Posts.AddAsync(newPost);
            await _db.SaveChangesAsync();
            string postIdval = Encryption.EncryptID(newPost.PostId.ToString(), LoginUserId.ToString());
            result = Result<string>.Success(postIdval);

            ///Save Policies
            await SavePostPolicies(newPost.PostId, 1, payload.viewPolicy);//Save View Policy
            await SavePostPolicies(newPost.PostId, 2, payload.reactPolicy);//Save React Policy
            await SavePostPolicies(newPost.PostId, 3, payload.commandPolicy);//Save Command Policy
            await SavePostPolicies(newPost.PostId, 4, payload.sharePolicy);//Save Share Policy
            /*int policyId = 1;
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
             */
            ///Save Post Images
            /*if (payload.imageData.Any())
            {
                string baseDirectory = _configuration["appSettings:UploadPath"] ?? throw new Exception("Invalid UploadPath");
                string uploadDirectory = _configuration["appSettings:PostImages"] ?? throw new Exception("Invalid function upload path.");
                string destDirectory = Path.Combine(baseDirectory, uploadDirectory);
                if (!Directory.Exists(destDirectory))
                {
                    Directory.CreateDirectory(destDirectory);
                }
                string filename = Globalfunction.NewUniqueFileName() + ".png";
                    string base64Str = item.imagebase64!;
                    byte[] bytes = Convert.FromBase64String(base64Str!);

                    string filePath = Path.Combine(destDirectory, filename);
                    if (filePath.Contains(".."))
                    { //if found .. in the file name or path
                        Log.Error("Invalid path " + filePath);
                    }
                    await System.IO.File.WriteAllBytesAsync(filePath, bytes);
                 
            string bucketname = _configuration.GetSection("Buckets:PostImages").Get<string>()!;
            foreach (var item in payload.imageData)
            {
                string uniquekey = Globalfunction.NewUniqueFileKey(item.ext!);
                await _kcAwsS3Service.CreateFileAsync(item.imagebase64!, bucketname, uniquekey, item.ext!);
                var newImage = new PostImage
                {
                    Url = uniquekey,
                    Description = item.description,
                    PostId = newPost.PostId,
                    CreatedDate = DateTime.UtcNow,
                };
                await _db.PostImages.AddAsync(newImage);
                await _db.SaveChangesAsync();
            }
        }
             */

            ///Create Post Balance
            foreach (var item in payload.postBalances)
            {
                int NMarkId = Convert.ToInt32(Encryption.DecryptID(item.MarkIdval, LoginUserId.ToString()));
                PostBalance newbalance = new PostBalance { 
                    PostId = newPost.PostId,
                    Balance = Encryption.EncryptID(item.Balance.ToString(), balanceSalt),
                    MarkId = NMarkId
                };
                await _db.PostBalances.AddAsync(newbalance);
                await _db.SaveChangesAsync();

                if(!string.IsNullOrEmpty(item.ToMarkIdval))
                {
                    int ToMarkId = Convert.ToInt32(Encryption.DecryptID(item.ToMarkIdval, LoginUserId.ToString()));
                    ///Create Exchange Rate
                    var exchange = await _db.ExchangeRates.Where(x =>
                        x.FromMarkId == NMarkId
                        && x.EventPostId == EventPostId
                        && x.ToMarkId == ToMarkId
                        && x.MinQuantity <= item.Balance)// Check MinQuantity condition
                        .OrderByDescending(x => x.MinQuantity)
                        .FirstOrDefaultAsync();
                    if(exchange is not null)
                    {
                        decimal bal = item.Balance * exchange.Rate;
                        PostBalance newbal = new PostBalance
                        {
                            PostId = newPost.PostId,
                            Balance = Encryption.EncryptID(bal.ToString(), balanceSalt),
                            MarkId = ToMarkId
                        };
                        await _db.PostBalances.AddAsync(newbal);
                        await _db.SaveChangesAsync();
                    }
                }
            }
            foreach (var item in payload.postTags)
            {
                int NTagId = Convert.ToInt32(Encryption.DecryptID(item.TagIdval, LoginUserId.ToString()));
                PostTag newtag = new PostTag { 
                    PostId = newPost.PostId,
                    EventTagId = item.IsUser ?  null : NTagId,
                    UserId = item.IsUser ? NTagId : null
                };
                await _db.PostTags.AddAsync(newtag);
                await _db.SaveChangesAsync();
            }
            var checkEventOwner = await (from _em in _db.EventMemberships
                                         join _ust in _db.UserTypes on _em.UserTypeId equals _ust.TypeId
                                         where _em.EventPostId == EventPostId
                                         && _em.UserId == CreatorId
                                         && _ust.Name.ToLower() == "owner"
                                         select new
                                         {
                                             LoginId = _em.UserId
                                         })
                                         .FirstOrDefaultAsync();
            if (checkEventOwner is not null && payload.CreatorIdval is null)
            {
                ///Already Approved because Post Creator is event owner
                int approvedStatus = await _db.StatusTypes
                    .Where(x=> x.StatusName.ToLower() == "approved")
                    .Select(x=> x.StatusId).FirstOrDefaultAsync();

                CollectPost newCollect = new CollectPost
                {

                    PostId = newPost.PostId,
                    Content = payload.Content,
                    //TagId = TagId,
                    EventPostId = EventPostId,
                    //CollectAmount = Encryption.EncryptID(payload.CollectAmount!.ToString()!, balanceSalt),
                    //MarkId=MarkId,
                    CreatorId = CreatorId,
                    StatusId = approvedStatus,
                };
                await _db.CollectPosts.AddAsync(newCollect);
                await _db.SaveChangesAsync();

                ///Update Event TotalBalance Amount and LastBalance Amount
                foreach (var item in payload.postBalances)
                {
                    int lMarkId = Convert.ToInt32(Encryption.DecryptID(item.MarkIdval, LoginUserId.ToString()));
                    await UpdateCollectBalance(EventPostId, lMarkId, item.Balance, balanceSalt);

                    if (!string.IsNullOrEmpty(item.ToMarkIdval))
                    {
                        int ToMarkId = Convert.ToInt32(Encryption.DecryptID(item.ToMarkIdval, LoginUserId.ToString()));
                        ///Create Exchange Rate
                        var exchange = await _db.ExchangeRates
                                        .Where(x =>
                                        x.FromMarkId == lMarkId
                                        && x.EventPostId == EventPostId
                                        && x.ToMarkId == ToMarkId
                                        && x.MinQuantity <= item.Balance) // Check MinQuantity condition
                                        .OrderByDescending(x => x.MinQuantity) // Order by MinQuantity in descending order
                                        .FirstOrDefaultAsync();
                        if (exchange is not null)
                        {
                            decimal bal = item.Balance * exchange.Rate;
                            await UpdateCollectBalance(EventPostId, ToMarkId, bal, balanceSalt);
                        }
                    }
                }

                PostPolicyPropertyPayload viewPolicy = payload.viewPolicy;
                ///Notifi the members if post privicy is not private
                if (viewPolicy.MaxCount == 0)/// maxcount(0) mean private
                {
                    List<int> channelMembers = await (from _ev in _db.Events
                                                      join _chan in _db.Channels on _ev.ChannelId equals _chan.ChannelId
                                                      join _chme in _db.ChannelMemberships on _chan.ChannelId equals _chme.ChannelId
                                                      where _ev.PostId == EventPostId
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
                        $"New Post",
                        $"{LoginName} Collected ",//{payload.CollectAmount}
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
                    Content = payload.Content,
                    //TagId = TagId,
                    EventPostId = EventPostId,
                    //CollectAmount = Encryption.EncryptID(payload.CollectAmount!.ToString()!, balanceSalt),
                    //MarkId = MarkId,
                    CreatorId = CreatorId,
                    StatusId = pendingStatus,
                };
                await _db.CollectPosts.AddAsync(newCollect);
                await _db.SaveChangesAsync();


                ///Notifie to event admins{Note: don't Channel Admins}
                List<int> admins = await (from eme in _db.EventMemberships
                                          join _ut in _db.UserTypes on eme.UserTypeId equals _ut.TypeId
                                          where eme.EventPostId == EventPostId
                                          && (_ut.Name.ToLower() == "admin" || _ut.Name.ToLower() == "owner")
                                          select eme.UserId).ToListAsync();
                if(admins.Contains(LoginUserId))
                {
                    admins.Remove(LoginUserId);
                }
                admins.Distinct();
                Event? parentEvent = await _db.Events
                    .Where(x => x.PostId == EventPostId)
                    .FirstOrDefaultAsync();
                string? LoginName = await _db.Users.Where(x => x.UserId == LoginUserId)
                        .Select(x => x.Name).FirstOrDefaultAsync();
                await _notificationmanager.SaveNotification(
                    admins,
                    LoginUserId,
                    $"Requested to Collect Posts in {parentEvent?.EventName}",
                    $"{LoginName} Collected  in {parentEvent?.EventName}",//{payload.CollectAmount}
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
        }
        catch (Exception ex)
        {
            result = Result<string>.Error(ex);
        }
        return result;
    }

    private async Task UpdateCollectBalance(int EventPostId,int MarkId,decimal CollectAmount,string balanceSalt)
    {

        ///Update Event TotalBalance Amount and LastBalance Amount
        EventMarkBalance? eventBalance = await _db.EventMarkBalances.Where(x => x.EventPostId == EventPostId && x.MarkId == MarkId).FirstOrDefaultAsync();
        if (eventBalance is not null)
        {
            ///update
            decimal EventTotalBalance = Globalfunction.StringToDecimal(
                Encryption.DecryptID(eventBalance.TotalBalance.ToString(), balanceSalt));
            decimal EventLastBalance = Globalfunction.StringToDecimal(
                    Encryption.DecryptID(eventBalance.LastBalance.ToString(), balanceSalt));
            if (CollectAmount > 0)
            {
                EventTotalBalance = EventTotalBalance + CollectAmount;
            }
            EventLastBalance = EventLastBalance + CollectAmount;
            eventBalance.TotalBalance = Encryption.EncryptID(EventTotalBalance.ToString(), balanceSalt);
            eventBalance.LastBalance = Encryption.EncryptID(EventLastBalance.ToString(), balanceSalt);
            await _db.SaveChangesAsync();
        }
        else
        {
                ///create new
                EventMarkBalance newBalance = new EventMarkBalance
                {
                    EventPostId = EventPostId,
                    MarkId = MarkId,
                    TotalBalance = Encryption.EncryptID(CollectAmount > 0 ? CollectAmount.ToString() : "0.0", balanceSalt),
                    LastBalance = Encryption.EncryptID(CollectAmount.ToString(), balanceSalt),
                    TargetBalance = null
                };
                await _db.EventMarkBalances.AddAsync(newBalance);
                await _db.SaveChangesAsync();

        }
        /*
         old code
         Event? parentEvent = await _db.Events
             .Where(x=> x.PostId == EventPostId)
             .FirstOrDefaultAsync();
         if (parentEvent is not null)
         {
             decimal EventTotalBalance = Globalfunction.StringToDecimal(
                 Encryption.DecryptID(parentEvent.TotalBalance.ToString(), balanceSalt));
             decimal EventLastBalance = Globalfunction.StringToDecimal(
                     Encryption.DecryptID(parentEvent.LastBalance.ToString(), balanceSalt));
             EventTotalBalance = EventTotalBalance + payload.CollectAmount;
             EventLastBalance = EventLastBalance + payload.CollectAmount;
             parentEvent.TotalBalance = Encryption.EncryptID(EventTotalBalance.ToString(), balanceSalt);
             parentEvent.LastBalance = Encryption.EncryptID(EventLastBalance.ToString(), balanceSalt);
             await _db.SaveChangesAsync();
         }
         */

        ///Update Channel TotalBalance Amount and LastBalance Amount
        ChannelMarkBalance? chanBalance = await (from _chan in _db.Channels
                                                 join _ev in _db.Events on _chan.ChannelId equals _ev.ChannelId
                                                 join _chanb in _db.ChannelMarkBalances on _chan.ChannelId equals _chanb.ChannelId
                                                 where _ev.PostId == EventPostId && _chanb.MarkId == MarkId
                                                 select _chanb).FirstOrDefaultAsync();
        if (chanBalance is not null)
        {

            decimal ChannelTotalBalance = Globalfunction.StringToDecimal(
                Encryption.DecryptID(chanBalance.TotalBalance!.ToString(), balanceSalt));
            decimal ChannelLastBalance = Globalfunction.StringToDecimal(
                    Encryption.DecryptID(chanBalance.LastBalance!.ToString(), balanceSalt));
            if(CollectAmount > 0)
            {
                ChannelTotalBalance = ChannelTotalBalance + CollectAmount;
            }
            ChannelLastBalance = ChannelLastBalance + CollectAmount;
            chanBalance.TotalBalance = Encryption.EncryptID(ChannelTotalBalance.ToString(), balanceSalt);
            chanBalance.LastBalance = Encryption.EncryptID(ChannelLastBalance.ToString(), balanceSalt);
            await _db.SaveChangesAsync();
        }
        else
        {
                int ChannelId = await (from _chan in _db.Channels
                                       join _ev in _db.Events on _chan.ChannelId equals _ev.ChannelId
                                       where _ev.PostId == EventPostId
                                       select _chan.ChannelId).FirstOrDefaultAsync();
                if (ChannelId > 0)
                {

                    ChannelMarkBalance newBalance = new ChannelMarkBalance
                    {
                        ChannelId = ChannelId,
                        MarkId = MarkId,
                        TotalBalance = Encryption.EncryptID(CollectAmount > 0 ? CollectAmount.ToString() : "0.0", balanceSalt),
                        LastBalance = Encryption.EncryptID(CollectAmount.ToString(), balanceSalt),
                    };
                    await _db.ChannelMarkBalances.AddAsync(newBalance);
                    await _db.SaveChangesAsync();
                }
        }
        /*
         * old code
        Channel? parentChannel = await (from _chan in _db.Channels
                                        join _ev in _db.Events on _chan.ChannelId equals _ev.ChannelId
                                        where _ev.PostId == EventPostId
                                        select _chan)
                                       .FirstOrDefaultAsync();
        if(parentChannel is not null)
        {
            decimal ChannelTotalBalance = Globalfunction.StringToDecimal(
                Encryption.DecryptID(parentChannel.TotalBalance!.ToString(), balanceSalt));
            decimal ChannelLastBalance = Globalfunction.StringToDecimal(
                    Encryption.DecryptID(parentChannel.LastBalance!.ToString(), balanceSalt));
            ChannelTotalBalance = ChannelTotalBalance + payload.CollectAmount;
            ChannelLastBalance = ChannelLastBalance + payload.CollectAmount;
            parentChannel.TotalBalance = Encryption.EncryptID(ChannelTotalBalance.ToString(), balanceSalt);
            parentChannel.LastBalance = Encryption.EncryptID(ChannelLastBalance.ToString(), balanceSalt);
            await _db.SaveChangesAsync();
        }
         */
    }

    private async Task SavePostPolicies(int postid, int policyId, PostPolicyPropertyPayload policy)
    {

        PostPolicyProperty newPostPolicy = new PostPolicyProperty
        {
            PostId = postid,
            PolicyId = policyId,
            MaxCount = policy.MaxCount,
            StartDate = policy.StartDate,
            EndDate = policy.EndDate,
            GroupMemberOnly = policy.GroupMemberOnly,
            FriendOnly = policy.FriendOnly
        };
        await _db.PostPolicyProperties.AddAsync(newPostPolicy);
        await _db.SaveChangesAsync();
    }

    public async Task<Result<string>> UploadCollectAttachFile(IFormFile file, string PostIdval, int LoginUserID)
    {
        Result<string> result = null;
        try
        {
                int PostId = Convert.ToInt32(Encryption.DecryptID(PostIdval, LoginUserID.ToString()));
                var post = await _db.Posts.Where(x => x.PostId == PostId).FirstOrDefaultAsync();
                if(post is not null)
                {
                    string bucketname = _configuration.GetSection("Buckets:PostImages").Get<string>()!;


                    // Get the file extension
                    string ext = Path.GetExtension(file.FileName);
                    string uniquekey = Globalfunction.NewUniqueFileKey(ext);
                    Result<string> res= await _kcAwsS3Service.CreateFileAsync(file, bucketname, uniquekey, ext);
                if (res.IsSuccess)
                {
                    var newImage = new PostImage
                    {
                        Url = uniquekey,
                        Description = "",
                        PostId = post.PostId,
                        CreatedDate = DateTime.UtcNow,
                    };
                    await _db.PostImages.AddAsync(newImage);
                    await _db.SaveChangesAsync();
                    result = Result<string>.Success("Upload Success");
                }
                else
                {
                    result = res;
                }
                }
                else
                {
                    result = Result<string>.Error("Post Not Found");
                }
        }
        catch (Exception ex)
        {
            result = Result<string>.Error(ex);
        }
        return result;
    }
    public async Task<Result<Pagination>> ReviewPostsList(ReviewPostPayload payload, int LoginUserId)
    {
        Result<Pagination> result = null;//string EventPostIdval,string StatusName
        try
        {
            string EventPostIdval = payload.EventPostIdval!;
            string StatusName = payload.Status!;
            string balanceSalt = _configuration["appSettings:BalanceSalt"] ?? throw new Exception("Invalid Balance Salt");
            int EventPostId = Convert.ToInt32(Encryption.DecryptID(EventPostIdval, LoginUserId.ToString()));
            var query = await (from _event in _db.Events
                                                    join _cp in _db.CollectPosts on _event.PostId equals _cp.EventPostId
                                                    join _post in _db.Posts on _cp.PostId equals _post.PostId
                                                    join _evpost in _db.Posts on _cp.EventPostId equals _evpost.PostId
                                                    join _ev in _db.Events on _evpost.PostId equals _ev.PostId
                                                    //join _mk in _db.Marks on _cp.MarkId equals _mk.MarkId
                                                    //join _allow in _db.EventAllowedMarks on _mk.MarkId equals _allow.MarkId
                                                    //join _total in _db.EventMarkBalances on _mk.MarkId equals _total.MarkId
                                                    join _creator in _db.Users on _cp.CreatorId equals _creator.UserId
                                                    join _status in _db.StatusTypes on _cp.StatusId equals _status.StatusId
                                                    join _em in _db.EventMemberships on _event.PostId equals _em.EventPostId
                                                    join _logu in _db.Users on _em.UserId equals _logu.UserId
                                                    join _ut in _db.UserTypes on _em.UserTypeId equals _ut.TypeId
                                                    orderby _post.ModifiedDate descending
                                                    where _event.PostId == EventPostId && _post.Inactive == false &&
                                                    _status.StatusName.ToLower() == StatusName.ToLower() &&
                                                    _logu.UserId == LoginUserId &&
                                                    (_ut.Name.ToLower() == "admin" ||
                                                    _ut.Name.ToLower() == "owner") 
                               select new 
                                                    {
                                                        PostId = _post.PostId,
                                                        //PostIdval = Encryption.EncryptID(_post.PostId.ToString(), LoginUserId.ToString()),
                                                        Content = _cp.Content ?? "",
                                                        //TagIdval = _cp.TagId != null ? Encryption.EncryptID(_cp.TagId!.Value.ToString(), LoginUserId.ToString()) : "",
                                                        //TagName = _cp.TagId != null ? _db.PostTags.Where(x => x.TagId == _cp.TagId!).Select(x => x.TagName).FirstOrDefault() : "",
                                                        CreatorIdval = Encryption.EncryptID(_creator.UserId.ToString(), LoginUserId.ToString()),
                                                        CreatorName = _creator.Name,
                                                        //CollectAmount = Globalfunction.StringToDecimal(Encryption.DecryptID(_cp.CollectAmount, balanceSalt)),
                                                        EventPostId = _evpost.PostId,
                                                        EventName = _ev.EventName,
                                                        //EventTotalAmount = Globalfunction.StringToDecimal(Encryption.DecryptID(_total.TotalBalance, balanceSalt)) + Globalfunction.StringToDecimal(Encryption.DecryptID(_cp.CollectAmount, balanceSalt)),
                                                        //IsoCode = _mk.Isocode,
                                                        //AllowedMarkName = _allow.AllowedMarkName,
                                                        CreatedDate = _post.CreatedDate,
                                                        ImageResponse = _db.PostImages.Where(x => x.PostId == _post.PostId)
                                                                                    .Select(x => new PostImageResponse
                                                                                    {
                                                                                        ImageIdval = Encryption.EncryptID(x.ImageId.ToString(), LoginUserId.ToString()),
                                                                                        ImageUrl = x.Url,
                                                                                        Description = x.Description
                                                                                    }).ToList(),
                                                        CreatorImageUrl = _db.UserProfiles.Where(x => x.UserId == _creator.UserId)
                                                        .OrderByDescending(x=> x.CreatedDate)
                                                        .Select(x => x.Url).LastOrDefault()
                                                    }).ToListAsync();
            List<ReviewPostResponse> postReviews = new List<ReviewPostResponse>();
            foreach (var item in query)
            {
                List<PostBalanceResponse> balancereviews = await (from _colbal in _db.PostBalances
                                                 //join _allow in _db.EventAllowedMarks on _colbal.MarkId equals _allow.MarkId
                                                 join _evbalance in _db.EventMarkBalances on _colbal.MarkId equals _evbalance.MarkId
                                                 join _mark in _db.Marks on _colbal.MarkId equals _mark.MarkId
                                                 where _colbal.PostId == item.PostId && _evbalance.EventPostId == item.EventPostId
                                                 select new PostBalanceResponse
                                                 {
                                                     CollectAmount = Globalfunction.StringToDecimal(Encryption.DecryptID(_colbal.Balance, balanceSalt)),
                                                     EventTotalAmount = StatusName.ToLower() == "approved" ? Globalfunction.StringToDecimal(Encryption.DecryptID(_evbalance.TotalBalance, balanceSalt))
                                                     : (Globalfunction.StringToDecimal(Encryption.DecryptID(_evbalance.TotalBalance, balanceSalt)) + Globalfunction.StringToDecimal(Encryption.DecryptID(_colbal.Balance, balanceSalt))),
                                                     IsoCode = _mark.Isocode,
                                                     MarkName = _mark.MarkName
                                                 }).ToListAsync();
                List<PostTagResponse> postTags = await (from _ptag in _db.PostTags
                                                  join _evtag in _db.EventTags on _ptag.EventTagId equals _evtag.EventTagId
                                                  where _ptag.PostId == item.PostId
                                                  select new PostTagResponse
                                                  {
                                                      PostTagIdval = Encryption.EncryptID(_ptag.PostTagId.ToString(), LoginUserId.ToString()),
                                                      TagName = _evtag.TagName
                                                  }).ToListAsync();
                ReviewPostResponse newpostReview = new ReviewPostResponse
                {
                    PostIdval = Encryption.EncryptID(item.PostId.ToString(),LoginUserId.ToString()),
                    Content = item.Content,
                    CreatorIdval = item.CreatorIdval,
                    CreatorName = item.CreatorName,
                    CreatorImageUrl = "",
                    EventIdval = Encryption.EncryptID(item.EventPostId.ToString(), LoginUserId.ToString()),
                    EventName = item.EventName,
                    CreatedDate= item.CreatedDate,
                    postTagRes = postTags,
                    postBalanceRes = balancereviews,
                    ImageResponse= item.ImageResponse
                };
                postReviews.Add(newpostReview);
}
            Pagination data = RepoFunService.getWithPagination(payload.PageNumber, payload.PageSize, postReviews);
            result = Result<Pagination>.Success(data);
        }
        catch (Exception ex)
        {
            result = Result<Pagination>.Error(ex);
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
                        var upcoll = await _db.PostBalances.Where(x => x.PostId == collectPost.PostId)
                            .Select(x => new
                            {
                                CollectBal = x.Balance,
                                MarkId = x.MarkId
                            }).ToListAsync();
                        foreach (var up in upcoll)
                        {
                            int MId = up.MarkId;
                            decimal bal = Globalfunction.StringToDecimal(
                                                            Encryption.DecryptID(up.CollectBal, balanceSalt));
                            await UpdateCollectBalance(collectPost.EventPostId, MId, bal, balanceSalt);
                        }
                        /*decimal collectAmount = Globalfunction.StringToDecimal(
                                                            Encryption.DecryptID(collectPost.CollectAmount, balanceSalt));
                        var eventData = await (from _post in _db.Posts
                                                 join _collPost in _db.CollectPosts on _post.PostId equals _collPost.PostId
                                                 join _eve in _db.Events on _collPost.EventPostId equals _eve.PostId
                                                 where _post.PostId == PostId
                                                 select new
                                                 {
                                                     EventPostId = _eve.PostId,
                                                     MarkId = _collPost.MarkId
                                                 }).FirstOrDefaultAsync();
                        if(eventData is not null)
                        {
                            await UpdateCollectBalance(eventData.EventPostId, eventData.MarkId, collectAmount, balanceSalt);
                        }
                         */

                        /*
                         * old code
                        var eventdata = await (from _post in _db.Posts
                                               join _collPost in _db.CollectPosts on _post.PostId equals _collPost.PostId
                                               join _eve in _db.Events on _collPost.EventPostId equals _eve.PostId
                                               where _post.PostId == PostId
                                               select _eve).FirstOrDefaultAsync();
                        if(eventdata is not null)
                        {
                            decimal oldTotalAmount = Globalfunction.StringToDecimal(
                                                            Encryption.DecryptID(eventdata.TotalBalance, balanceSalt));
                            decimal oldLastAmount = Globalfunction.StringToDecimal(
                                                            Encryption.DecryptID(eventdata.LastBalance, balanceSalt));
                            decimal newTotalAmount = oldTotalAmount + collectAmount;
                            decimal newLastAmount = oldLastAmount + collectAmount;
                            eventdata.TotalBalance = Encryption.EncryptID(newTotalAmount.ToString(), balanceSalt);
                            eventdata.LastBalance = Encryption.EncryptID(newLastAmount.ToString(), balanceSalt);
                            await _db.SaveChangesAsync();
                        }
                        var channelData = await (from _post in _db.Posts
                                                 join _collP in _db.CollectPosts on _post.PostId equals _collP.PostId
                                               join _eve in _db.Events on _collP.EventPostId equals _eve.PostId
                                               join _chn in _db.Channels on _eve.ChannelId equals _chn.ChannelId
                                               where _post.PostId == PostId
                                               select _chn).FirstOrDefaultAsync();
                        if (channelData is not null)
                        {
                            decimal oldTotalAmount = Globalfunction.StringToDecimal(
                                                            Encryption.DecryptID(channelData.TotalBalance!, balanceSalt));
                            decimal oldLastAmount = Globalfunction.StringToDecimal(
                                                            Encryption.DecryptID(channelData.LastBalance!, balanceSalt));
                            decimal newTotalAmount = oldTotalAmount + collectAmount;
                            decimal newLastAmount = oldLastAmount + collectAmount;
                            channelData.TotalBalance = Encryption.EncryptID(newTotalAmount.ToString(), balanceSalt);
                            channelData.LastBalance = Encryption.EncryptID(newLastAmount.ToString(), balanceSalt);
                            await _db.SaveChangesAsync();
                        }

                         */

                    }
                    if (item.AppRejStatus == 2)///Reject
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
    /// <summary>
    /// Response Posts form dashboard.This posts are posts list that LoginUser can view
    /// </summary>
    /// <param name="LoginUserId"></param>
    /// <returns></returns>
    public async Task<Result<Pagination>> GetDashboardPosts(int LoginUserId,int pageNumber,int pageSize)
    {
        Result<Pagination> result = null;
        try
        {
            string balanceSalt = _configuration["appSettings:BalanceSalt"] ?? throw new Exception("Invalid Balance Salt");


            ///post must active
            ///post status must approved
            ///post view count must less that equeal maxcount or view must null
            ///Check User Post Interactions
            ///old Code
            /*
             var posts = await (from _post in _db.Posts
                               join _colP in _db.CollectPosts on _post.PostId equals _colP.PostId
                               where _post.Inactive == false
                               select new
                               {
                                   Post = _colP,
                                   ModifiedDate = _post.ModifiedDate,
                                   CreatedDate = _post.CreatedDate,
                                   ViewPolicies = _db.PostPolicyProperties.Where(p => p.PostId == _post.PostId && p.PolicyId == 1).FirstOrDefault(),
                                   LikePolicies = _db.PostPolicyProperties.Where(p => p.PostId == _post.PostId && p.PolicyId == 2).FirstOrDefault(),
                                   CommandPolicies = _db.PostPolicyProperties.Where(p => p.PostId == _post.PostId && p.PolicyId == 3).FirstOrDefault(),
                                   SharePolicies = _db.PostPolicyProperties.Where(p => p.PostId == _post.PostId && p.PolicyId == 4).FirstOrDefault(),
                                   UserInteractions = _db.UserPostInteractions.Where(p=> p.PostId == _post.PostId && p.UserId == LoginUserId).FirstOrDefault(),
                                   Views = _db.PostViewers.Where(p => p.PostId == _post.PostId).Count(),
                                   Likes = _db.Reacts.Where(p => p.PostId == _post.PostId).Count(),
                                   Commands = _db.PostCommands.Where(p => p.PostId == _post.PostId).Count(),
                                   Shares = _db.PostShares.Where(p => p.PostId == _post.PostId).Count(),
                               })
                   .ToListAsync();
            var query = (from _post in posts
                         //join _evbalance in _db.EventMarkBalances on _post.Post.EventPostId equals _evbalance.EventPostId
                         //join _mark in _db.Marks on _evbalance.MarkId equals _mark.MarkId
                         //join _allow in _db.EventAllowedMarks on _evbalance.MarkId equals _allow.MarkId
                         join _postStatus in _db.StatusTypes on _post.Post.StatusId equals _postStatus.StatusId
                         join _event in _db.Events on _post.Post.EventPostId equals _event.PostId
                         join _channel in _db.Channels on _event.ChannelId equals _channel.ChannelId
                         join _chanme in _db.ChannelMemberships on _event.ChannelId equals _chanme.ChannelId
                         join _creator in _db.Users on _post.Post.CreatorId equals _creator.UserId
                         join _poimg in _db.PostImages on _post.Post.PostId equals _poimg.PostId into postImages
                         where _event.StartDate < DateTime.UtcNow && _event.EndDate > DateTime.UtcNow &&
                         _postStatus.StatusName.ToLower() == "approved" && 
                         //_evbalance.MarkId == _post.Post.MarkId &&
                         (_post.UserInteractions != null ? _post.UserInteractions.VisibilityPercentage < 70 : true) &&
                         (_post.ViewPolicies.GroupMemberOnly != null && _post.ViewPolicies.GroupMemberOnly == true ? _chanme.UserId == LoginUserId : true) &&
                         (_post.ViewPolicies.MaxCount != null ? _post.ViewPolicies.MaxCount > _post.Views : true)
                         select new 
                         {
                             PostId = _post.Post.PostId,
                             Content = _post.Post.Content,
                             ChannelIdval = Encryption.EncryptID(_channel.ChannelId.ToString(), LoginUserId.ToString()),
                             ChannelName = _channel.ChannelName,
                             EventPostId = _event.PostId,
                             EventName = _event.EventName,
                             //TagIdval = _post.Post.TagId != null ? Encryption.EncryptID(_post.Post.TagId.ToString()!, LoginUserId.ToString()) : null,
                             //TagName = _post.Post.TagId != null ? _db.PostTags.Where(x => x.TagId == _post.Post.TagId).Select(x => x.TagName).FirstOrDefault() : null,
                             CreatorIdval = Encryption.EncryptID(_creator.UserId.ToString(), LoginUserId.ToString()),
                             CreatorName = _creator.Name,
                             //CollectAmount = Globalfunction.StringToDecimal(Encryption.DecryptID(_post.Post.CollectAmount, balanceSalt)),
                             //EventTotalAmount = Globalfunction.StringToDecimal(Encryption.DecryptID(_evbalance.TotalBalance, balanceSalt)),
                             //IsoCode = _mark.Isocode,
                             //AllowedMarkName = _allow.AllowedMarkName,
                             ModifiedDate = _post.ModifiedDate,
                             CreatedDate = _post.CreatedDate,
                             ViewTotalCount = _post.Views,
                             LikeTotalCount = _post.Likes,
                             CommandTotalCount = _post.Commands,
                             ShareTotalCount = _post.Shares,
                             Selected = (_db.Reacts.Where(x => x.UserId == LoginUserId && _post.Post.PostId == x.PostId).FirstOrDefault() != null ? true : false),
                             CanLike = (_post.LikePolicies.GroupMemberOnly != null && _post.LikePolicies.GroupMemberOnly == true ? _chanme.UserId == LoginUserId : true) &&
                              (_post.LikePolicies.MaxCount != null ? _post.LikePolicies.MaxCount > _post.Likes : true),
                             CanCommand = (_post.CommandPolicies.GroupMemberOnly != null && _post.CommandPolicies.GroupMemberOnly == true ? _chanme.UserId == LoginUserId : true) &&
                             (_post.CommandPolicies.MaxCount != null ? _post.CommandPolicies.MaxCount > _post.Commands : true),
                             CanShare = (_post.SharePolicies.GroupMemberOnly != null && _post.SharePolicies.GroupMemberOnly == true ? _chanme.UserId == LoginUserId : true) &&
                             (_post.SharePolicies.MaxCount != null ? _post.SharePolicies.MaxCount > _post.Shares : true),
                             ImageResponse = postImages.Select(img => new PostImageResponse
                             {
                                 ImageIdval = Encryption.EncryptID(img.PostId.ToString(), LoginUserId.ToString()),
                                 ImageUrl = img.Url,
                                 Description = img.Description
                             }).ToList()
                         })
                               .Skip((pageNumber - 1) * pageSize)
                               .Take(pageSize)
                               .ToList();
            List<DashboardPostsResponse> dashPost = new List<DashboardPostsResponse>();
            foreach (var item in query)
            {
                List<PostTagResponse> postTags = await (from _potag in _db.PostTags
                                                        join _evtag in _db.EventTags on _potag.EventTagId equals _evtag.EventTagId
                                                        where _potag.PostId == item.PostId
                                                        select new PostTagResponse
                                                        {
                                                            PostTagIdval = Encryption.EncryptID(_potag.PostTagId.ToString(), LoginUserId.ToString()),
                                                            TagName = _evtag.TagName
                                                        }).ToListAsync();
                List<PostBalanceResponse> postBalances = await (from _pobal in _db.PostBalances
                                                                join _allow in _db.EventAllowedMarks on _pobal.MarkId equals _allow.MarkId
                                                                join _po in _db.EventMarkBalances on _allow.MarkId equals _po.MarkId
                                                                join _mark in _db.Marks on _pobal.MarkId equals _mark.MarkId
                                                                where _pobal.PostId == item.PostId && _allow.EventPostId == item.EventPostId
                                                                select new PostBalanceResponse
                                                                {
                                                                    CollectAmount = Globalfunction.StringToDecimal(Encryption.DecryptID(_pobal.Balance, balanceSalt)),
                                                                    EventTotalAmount = Globalfunction.StringToDecimal(Encryption.DecryptID(_po.TotalBalance, balanceSalt)),
                                                                    IsoCode = _mark.Isocode,
                                                                    AllowedMarkName = _allow.AllowedMarkName
                                                                }).ToListAsync();
                DashboardPostsResponse newPost = new DashboardPostsResponse
                {
                    PostIdval = Encryption.EncryptID(item.PostId.ToString(), LoginUserId.ToString()),
                    Content = item.Content,
                    ChannelIdval = item.ChannelIdval,
                    ChannelName = item.ChannelName,
                    EventPostIdval = Encryption.EncryptID(item.EventPostId.ToString(), LoginUserId.ToString()),
                    EventName = item.EventName,
                    CreatorIdval = item.CreatorIdval,
                    CreatorName = item.CreatorName,
                    ModifiedDate = item.ModifiedDate,
                    CreatedDate = item.CreatedDate,
                    LikeTotalCount = item.LikeTotalCount,
                    CommandTotalCount = item.CommandTotalCount,
                    ShareTotalCount = item.ShareTotalCount,
                    Selected = item.Selected,
                    CanLike = item.CanLike,
                    CanCommand = item.CanCommand,
                    CanShare = item.CanShare,
                    postTagRes = postTags,
                    postBalanceRes = postBalances,
                    ImageResponse = item.ImageResponse
                };
                dashPost.Add(newPost);
}
            result = Result<List<DashboardPostsResponse>>.Success(dashPost);*/
            List<dynamic> netResponse = new List<dynamic>();
            DateTime now = DateTime.UtcNow;
            var posts = await (from _post in _db.Posts
                               where _post.Inactive == false
                               orderby _post.ModifiedDate descending
                               select new
                               {
                                   PostId = _post.PostId,
                                   PostType = _post.PostType,
                                   ModifiedDate = _post.ModifiedDate,
                                   CreatedDate = _post.CreatedDate,
                                   ViewPolicies = _db.PostPolicyProperties.Where(p => p.PostId == _post.PostId && p.PolicyId == 1).FirstOrDefault(),
                                   LikePolicies = _db.PostPolicyProperties.Where(p => p.PostId == _post.PostId && p.PolicyId == 2).FirstOrDefault(),
                                   CommandPolicies = _db.PostPolicyProperties.Where(p => p.PostId == _post.PostId && p.PolicyId == 3).FirstOrDefault(),
                                   SharePolicies = _db.PostPolicyProperties.Where(p => p.PostId == _post.PostId && p.PolicyId == 4).FirstOrDefault(),
                                   UserInteractions = _db.UserPostInteractions.Where(p => p.PostId == _post.PostId && p.UserId == LoginUserId).FirstOrDefault(),
                                   Views = _db.PostViewers.Where(p => p.PostId == _post.PostId).Count(),
                                   Likes = _db.Reacts.Where(p => p.PostId == _post.PostId).Count(),
                                   Commands = _db.PostCommands.Where(p => p.PostId == _post.PostId).Count(),
                                   Shares = _db.PostShares.Where(p => p.PostId == _post.PostId).Count(),
                               })
                   .ToListAsync();
            foreach (var apost in posts)
            {
                if(apost.PostType == "eventpost")
                {
                    var eventQuery = await (from _ev in _db.Events
                                            join _cre in _db.Users on _ev.CreatorId equals _cre.UserId
                                            join _sta in _db.StatusTypes on _ev.StatusId equals _sta.StatusId
                                            join _meship in _db.ChannelMemberships on _ev.ChannelId equals _meship.ChannelId
                                            join _meStatus in _db.StatusTypes on _meship.StatusId equals _meStatus.StatusId
                                            join _usertype in _db.UserTypes on _meship.UserTypeId equals _usertype.TypeId
                                            where apost.PostId == _ev.PostId &&
                                                  _sta.StatusName.ToLower() == "approved"  
                                                  && _ev.StartDate <= now && _ev.EndDate >= now
                                                   && (apost.UserInteractions != null ? 
                                                   apost.UserInteractions.VisibilityPercentage < 70 : true) &&
                                                    (apost.ViewPolicies.GroupMemberOnly != null 
                                                    && apost.ViewPolicies.GroupMemberOnly == true ? 
                                                    (_meship.UserId == LoginUserId && _meStatus.StatusName.ToLower() == "approved") : true) &&
                                            (apost.ViewPolicies.MaxCount != null ? apost.ViewPolicies.MaxCount > apost.Views : true)
                                            select new
                                            {
                                                Event = _ev,
                                                Post = apost,
                                                Creator = _cre,
                                                CMemberShip = _meship,
                                                EventMarks = _ev.PostId,
                                                EventAddresses = _ev.PostId,
                                                EventFiles = _ev.PostId
                                            }).FirstOrDefaultAsync();

                    if (eventQuery is not null)
                    {
                        // Fetch EventMarks
                        var eventMarks = (from _eventMark in _db.EventMarkBalances
                                          join _mark in _db.Marks on _eventMark.MarkId equals _mark.MarkId
                                          join _evall in _db.EventAllowedMarks on _eventMark.MarkId equals _evall.MarkId
                                          where eventQuery.Event.PostId == _eventMark.EventPostId
                                          group new { _eventMark, _mark, _evall } by new { _eventMark.EventPostId, _eventMark.MarkId } into groupedMarks
                                          select new
                                          {
                                              EventPostId = groupedMarks.Key.EventPostId,
                                              Mark = new EventMarks
                                              {
                                                  MarkIdval = Encryption.EncryptID(groupedMarks.Key.MarkId.ToString(), LoginUserId.ToString()),
                                                  IsoCode = groupedMarks.First()._mark.Isocode,
                                                  MarkName = groupedMarks.First()._mark.MarkName,
                                                  AllowedMarkName = groupedMarks.Select(g => g._evall.AllowedMarkName).Distinct().FirstOrDefault(),
                                                  TotalBalance = Globalfunction.StringToDecimal(Encryption.DecryptID(groupedMarks.First()._eventMark.TotalBalance.ToString(), balanceSalt)),
                                                  LastBalance = Globalfunction.StringToDecimal(Encryption.DecryptID(groupedMarks.First()._eventMark.LastBalance.ToString(), balanceSalt)),
                                                  TargetBalance = groupedMarks.First()._eventMark.TargetBalance != null
                                                      ? Globalfunction.StringToDecimal(Encryption.DecryptID(groupedMarks.First()._eventMark.TargetBalance.ToString(), balanceSalt))
                                                      : null
                                              }
                                          }).ToList();


                        // Fetch EventAddresses
                        var eventAddresses = (from _add in _db.EventAddresses
                                              join _atype in _db.AddressTypes on _add.AddressId equals _atype.AddressId
                                              where eventQuery.Event.PostId == _add.EventPostId
                                              select new
                                              {
                                                  EventPostId = _add.EventPostId,
                                                  AddressResponse = new EventAddressResponse
                                                  {
                                                      EventAddressIdval = Encryption.EncryptID(_add.EventAddressId.ToString(), LoginUserId.ToString()),
                                                      Address = _add.AddressName,
                                                      AddresstypeName = _atype.Address
                                                  }
                                              }).ToList();

                        // Fetch EventFiles
                        var eventFiles = _db.EventFiles
                                            .Where(x => eventQuery.Event.PostId == x.EventPostId)
                                            .Select(x => new
                                            {
                                                EventPostId = x.EventPostId,
                                                FileInfo = new EventFileInfo
                                                {
                                                    fileIdval = Encryption.EncryptID(x.UrlId.ToString(), LoginUserId.ToString()),
                                                    imgfilename = x.Url,
                                                    imgDescription = x.UrlDescription
                                                }
                                            }).ToList();

                        bool IsMember = await _db.ChannelMemberships
                            .Where(x => x.ChannelId == eventQuery.Event.ChannelId
                            && x.UserId == LoginUserId).FirstOrDefaultAsync() != null;
                        var finalResult = new DashboardEventPostResponse
                        {
                            PostType = eventQuery.Post.PostType,
                            ChannelIdval = Encryption.EncryptID(eventQuery.Event.ChannelId.ToString(), LoginUserId.ToString()),
                            EventPostIdval = Encryption.EncryptID(eventQuery.Event.PostId.ToString(), LoginUserId.ToString()),
                            EventName = eventQuery.Event.EventName,
                            EventDescrition = eventQuery.Event.EventDescription,
                            CreatorIdval = Encryption.EncryptID(eventQuery.Creator.UserId.ToString(), LoginUserId.ToString()),
                            CreatorName = eventQuery.Creator.Name,
                            StartDate = eventQuery.Event.StartDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                            EndDate = eventQuery.Event.EndDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                            //ModifiedDate = e.Post.ModifiedDate,//.ToString("yyyy-MM-ddTHH:mm:ss")
                            ModifiedDate = apost.ModifiedDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                            CreatedDate = apost.CreatedDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                            ViewTotalCount = apost.Views,
                            LikeTotalCount = apost.Likes,
                            CommandTotalCount = apost.Commands,
                            ShareTotalCount = apost.Shares,
                            IsMember = IsMember,
                            Selected = (_db.Reacts.Where(x => x.UserId == LoginUserId && apost.PostId == x.PostId).FirstOrDefault() != null ? true : false),
                            CanLike = (apost.LikePolicies.GroupMemberOnly != null && apost.LikePolicies.GroupMemberOnly == true ? eventQuery.CMemberShip.UserId == LoginUserId : true) &&
                                          (apost.LikePolicies.MaxCount != null ? apost.LikePolicies.MaxCount > apost.Likes : true),
                            CanCommand = (apost.CommandPolicies.GroupMemberOnly != null && apost.CommandPolicies.GroupMemberOnly == true ? eventQuery.CMemberShip.UserId == LoginUserId : true) &&
                                         (apost.CommandPolicies.MaxCount != null ? apost.CommandPolicies.MaxCount > apost.Commands : true),
                            CanShare = (apost.SharePolicies.GroupMemberOnly != null && apost.SharePolicies.GroupMemberOnly == true ? eventQuery.CMemberShip.UserId == LoginUserId : true) &&
                                         (apost.SharePolicies.MaxCount != null ? apost.SharePolicies.MaxCount > apost.Shares : true),
                            CanEdit = false,
                            // EventMarks
                            EventMarks = eventMarks.Where(m => m.EventPostId == eventQuery.Event.PostId).Select(m => m.Mark).ToList(),

                            // AddressResponse
                            AddressResponse = eventAddresses.Where(a => a.EventPostId == eventQuery.Event.PostId).Select(a => a.AddressResponse).ToList(),

                            // EventImageList
                            EventImageList = eventFiles.Where(f => f.EventPostId == eventQuery.Event.PostId).Select(f => f.FileInfo).ToList()

                        };
                        netResponse.Add(finalResult);
                    }
                }
                else if(apost.PostType == "collectPost")
                {
                    var query = (from _coll in _db.CollectPosts
                                 join _postStatus in _db.StatusTypes on _coll.StatusId equals _postStatus.StatusId
                                 join _event in _db.Events on _coll.EventPostId equals _event.PostId
                                 join _channel in _db.Channels on _event.ChannelId equals _channel.ChannelId
                                 join _chanme in _db.ChannelMemberships on _event.ChannelId equals _chanme.ChannelId
                                 join _meStatus in _db.StatusTypes on _chanme.StatusId equals _meStatus.StatusId
                                 join _creator in _db.Users on _coll.CreatorId equals _creator.UserId
                                 join _poimg in _db.PostImages on _coll.PostId equals _poimg.PostId into postImages
                                 where _event.StartDate < DateTime.UtcNow && _event.EndDate > DateTime.UtcNow &&
                                 _postStatus.StatusName.ToLower() == "approved" && _coll.PostId == apost.PostId &&
                                 (apost.UserInteractions != null ? apost.UserInteractions.VisibilityPercentage < 70 : true) &&
                                 (apost.ViewPolicies.GroupMemberOnly != null && apost.ViewPolicies.GroupMemberOnly == true ? 
                                 (_chanme.UserId == LoginUserId && _meStatus.StatusName.ToLower() == "approved") : true) &&
                                 (apost.ViewPolicies.MaxCount != null ? apost.ViewPolicies.MaxCount > apost.Views : true)
                                 select new
                                 {
                                     PostId = apost.PostId,
                                     PostType = apost.PostType,
                                     Content = _coll.Content,
                                     ChannelIdval = Encryption.EncryptID(_channel.ChannelId.ToString(), LoginUserId.ToString()),
                                     ChannelName = _channel.ChannelName,
                                     EventPostId = _event.PostId,
                                     EventName = _event.EventName,
                                     CreatorIdval = Encryption.EncryptID(_creator.UserId.ToString(), LoginUserId.ToString()),
                                     CreatorName = _creator.Name,
                                     ModifiedDate = apost.ModifiedDate,
                                     CreatedDate = apost.CreatedDate,
                                     ViewTotalCount = apost.Views,
                                     LikeTotalCount = apost.Likes,
                                     CommandTotalCount = apost.Commands,
                                     ShareTotalCount = apost.Shares,
                                     Selected = (_db.Reacts.Where(x => x.UserId == LoginUserId && apost.PostId == x.PostId).FirstOrDefault() != null ? true : false),
                                     CanLike = (apost.LikePolicies.GroupMemberOnly != null && apost.LikePolicies.GroupMemberOnly == true ? _chanme.UserId == LoginUserId : true) &&
                                      (apost.LikePolicies.MaxCount != null ? apost.LikePolicies.MaxCount > apost.Likes : true),
                                     CanCommand = (apost.CommandPolicies.GroupMemberOnly != null && apost.CommandPolicies.GroupMemberOnly == true ? _chanme.UserId == LoginUserId : true) &&
                                     (apost.CommandPolicies.MaxCount != null ? apost.CommandPolicies.MaxCount > apost.Commands : true),
                                     CanShare = (apost.SharePolicies.GroupMemberOnly != null && apost.SharePolicies.GroupMemberOnly == true ? _chanme.UserId == LoginUserId : true) &&
                                     (apost.SharePolicies.MaxCount != null ? apost.SharePolicies.MaxCount > apost.Shares : true),
                                     ImageResponse = postImages.Select(img => new PostImageResponse
                                     {
                                         ImageIdval = Encryption.EncryptID(img.PostId.ToString(), LoginUserId.ToString()),
                                         ImageUrl = img.Url,
                                         Description = img.Description
                                     }).ToList()
                                 })
                               .ToList();
                    foreach (var item in query)
                    {
                        List<PostTagResponse> postTags = await (from _potag in _db.PostTags
                                                                join _evtag in _db.EventTags on _potag.EventTagId equals _evtag.EventTagId
                                                                where _potag.PostId == item.PostId
                                                                select new PostTagResponse
                                                                {
                                                                    PostTagIdval = Encryption.EncryptID(_potag.PostTagId.ToString(), LoginUserId.ToString()),
                                                                    TagName = _evtag.TagName
                                                                }).ToListAsync();
                        var rawData = await (
                    from _pobal in _db.PostBalances
                    join _po in _db.EventMarkBalances on _pobal.MarkId equals _po.MarkId
                    join _mark in _db.Marks on _pobal.MarkId equals _mark.MarkId
                    where _pobal.PostId == item.PostId && _po.EventPostId == item.EventPostId
                    select new
                    {
                        CollectAmount = _pobal.Balance,
                        EventTotalAmount = _po.TotalBalance,
                        IsoCode = _mark.Isocode,
                        MarkName = _mark.MarkName
                    }
                ).ToListAsync();

                        List<PostBalanceResponse> postBalances = rawData
                            .GroupBy(x => new { x.IsoCode, x.MarkName })
                            .Select(g => new PostBalanceResponse
                            {
                                IsoCode = g.Key.IsoCode,
                                MarkName = g.Key.MarkName,
                                CollectAmount = g.Sum(x => Globalfunction.StringToDecimal(Encryption.DecryptID(x.CollectAmount, balanceSalt))),
                                EventTotalAmount = Globalfunction.StringToDecimal(Encryption.DecryptID(g.First().EventTotalAmount, balanceSalt))
                            })
                            .ToList();
                        /*
                            List<PostBalanceResponse> postBalances = await (from _pobal in _db.PostBalances
                                                                        //join _allow in _db.EventAllowedMarks on _pobal.MarkId equals _allow.MarkId
                                                                        join _po in _db.EventMarkBalances on _pobal.MarkId equals _po.MarkId
                                                                        join _mark in _db.Marks on _pobal.MarkId equals _mark.MarkId
                                                                        where _pobal.PostId == item.PostId && _po.EventPostId == item.EventPostId
                                                                        select new PostBalanceResponse
                                                                        {
                                                                            CollectAmount = Globalfunction.StringToDecimal(Encryption.DecryptID(_pobal.Balance, balanceSalt)),
                                                                            EventTotalAmount = Globalfunction.StringToDecimal(Encryption.DecryptID(_po.TotalBalance, balanceSalt)),
                                                                            IsoCode = _mark.Isocode,
                                                                            MarkName = _mark.MarkName
                                                                        }).ToListAsync();*/
                        DashboardPostsResponse newPost = new DashboardPostsResponse
                        {
                            PostType=item.PostType,
                            PostIdval = Encryption.EncryptID(item.PostId.ToString(), LoginUserId.ToString()),
                            Content = item.Content,
                            ChannelIdval = item.ChannelIdval,
                            ChannelName = item.ChannelName,
                            EventPostIdval = Encryption.EncryptID(item.EventPostId.ToString(), LoginUserId.ToString()),
                            EventName = item.EventName,
                            CreatorIdval = item.CreatorIdval,
                            CreatorName = item.CreatorName,
                            ModifiedDate = item.ModifiedDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                            CreatedDate = item.CreatedDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                            LikeTotalCount = item.LikeTotalCount,
                            CommandTotalCount = item.CommandTotalCount,
                            ShareTotalCount = item.ShareTotalCount,
                            Selected = item.Selected,
                            CanLike = item.CanLike,
                            CanCommand = item.CanCommand,
                            CanShare = item.CanShare,
                            CanEdit= false,
                            postTagRes = postTags,
                            postBalanceRes = postBalances,
                            ImageResponse = item.ImageResponse
                        };
                        netResponse.Add(newPost);
                    }
                }
            }
            Pagination pagi = RepoFunService.getWithPagination(pageNumber, pageSize, netResponse);
            result = Result<Pagination>.Success(pagi);

        }
        catch (Exception ex)
        {
            result = Result<Pagination>.Error(ex);
        }
        return result;
    }

    public async Task<Result<Pagination>> GetPostsOrderByEvent(GetEventData payload, int LoginUserId)
    {
        Result<Pagination> result = null;
        try
        {
            string balanceSalt = _configuration["appSettings:BalanceSalt"] ?? throw new Exception("Invalid Balance Salt");
            int EventPostId = Convert.ToInt32(Encryption.DecryptID(payload.EventIdval!.ToString(), LoginUserId.ToString()));
            int pageNumber = payload.pageNumber;
            int pageSize = payload.pageSize;
            ///post must active
            ///post status must approved
            ///post view count must less that equeal maxcount or view must null
            ///Check User Post Interactions
            var posts = await (from _post in _db.Posts
                               join _colP in _db.CollectPosts on _post.PostId equals _colP.PostId
                               where _post.Inactive == false
                               select new
                               {
                                   Post = _colP,
                                   ModifiedDate = _post.ModifiedDate,
                                   CreatedDate = _post.CreatedDate,
                                   ViewPolicies = _db.PostPolicyProperties.Where(p => p.PostId == _post.PostId && p.PolicyId == 1).FirstOrDefault(),
                                   LikePolicies = _db.PostPolicyProperties.Where(p => p.PostId == _post.PostId && p.PolicyId == 2).FirstOrDefault(),
                                   CommandPolicies = _db.PostPolicyProperties.Where(p => p.PostId == _post.PostId && p.PolicyId == 3).FirstOrDefault(),
                                   SharePolicies = _db.PostPolicyProperties.Where(p => p.PostId == _post.PostId && p.PolicyId == 4).FirstOrDefault(),
                                   //UserInteractions = _db.UserPostInteractions.Where(p => p.PostId == _post.PostId && p.UserId == LoginUserId).FirstOrDefault(),
                                   Views = _db.PostViewers.Where(p => p.PostId == _post.PostId).Count(),
                                   Likes = _db.Reacts.Where(p => p.PostId == _post.PostId).Count(),
                                   Commands = _db.PostCommands.Where(p => p.PostId == _post.PostId).Count(),
                                   Shares = _db.PostShares.Where(p => p.PostId == _post.PostId).Count(),
                               })
                   .ToListAsync();

            var query = (from _post in posts
                         join _postStatus in _db.StatusTypes on _post.Post.StatusId equals _postStatus.StatusId
                         join _event in _db.Events on _post.Post.EventPostId equals _event.PostId
                         join _channel in _db.Channels on _event.ChannelId equals _channel.ChannelId
                         join _chanme in _db.ChannelMemberships on _event.ChannelId equals _chanme.ChannelId
                         join _meStatus in _db.StatusTypes on _chanme.StatusId equals _meStatus.StatusId
                         join _creator in _db.Users on _post.Post.CreatorId equals _creator.UserId
                         join _poimg in _db.PostImages on _post.Post.PostId equals _poimg.PostId into postImages
                         where _event.PostId == EventPostId &&
                         //_event.StartDate < DateTime.UtcNow && _event.EndDate > DateTime.UtcNow &&
                         _postStatus.StatusName.ToLower() == "approved" &&
                         //(_post.UserInteractions != null ? _post.UserInteractions.VisibilityPercentage < 70 : true) &&
                         (_post.ViewPolicies.GroupMemberOnly != null && _post.ViewPolicies.GroupMemberOnly == true ? 
                         (_chanme.UserId == LoginUserId && _meStatus.StatusName.ToLower() == "approved") : true)
                         //(_post.ViewPolicies.MaxCount != null ? _post.ViewPolicies.MaxCount > _post.Views : true)
                         orderby _post.ModifiedDate descending
                         select new 
                         {
                             PostId = _post.Post.PostId,
                             Content = _post.Post.Content,
                             ChannelIdval = Encryption.EncryptID(_channel.ChannelId.ToString(), LoginUserId.ToString()),
                             ChannelName = _channel.ChannelName,
                             EventPostId = _event.PostId,
                             EventName = _event.EventName,
                             //TagIdval = _post.Post.TagId != null ? Encryption.EncryptID(_post.Post.TagId.ToString()!, LoginUserId.ToString()) : null,
                             //TagName = _post.Post.TagId != null ? _db.PostTags.Where(x => x.TagId == _post.Post.TagId).Select(x => x.TagName).FirstOrDefault() : null,
                             CreatorIdval = Encryption.EncryptID(_creator.UserId.ToString(), LoginUserId.ToString()),
                             CreatorName = _creator.Name,
                             //CollectAmount = Globalfunction.StringToDecimal(Encryption.DecryptID(_coll.CollectAmount, balanceSalt)),
                             //EventTotalAmount = Globalfunction.StringToDecimal(Encryption.DecryptID(_evbalance.TotalBalance, balanceSalt)),
                             //IsoCode = _mark.Isocode,
                             //AllowedMarkName = _allow.AllowedMarkName,
                             ModifiedDate = _post.ModifiedDate,
                             CreatedDate = _post.CreatedDate,
                             ViewTotalCount = _post.Views,
                             LikeTotalCount = _post.Likes,
                             CommandTotalCount = _post.Commands,
                             ShareTotalCount = _post.Shares,
                             Selected = (_db.Reacts.Where(x=> x.UserId == LoginUserId && _post.Post.PostId==x.PostId).FirstOrDefault() != null ? true : false),
                             CanLike = (_post.LikePolicies.GroupMemberOnly != null && _post.LikePolicies.GroupMemberOnly == true ? _chanme.UserId == LoginUserId : true) &&
                              (_post.LikePolicies.MaxCount != null ? _post.LikePolicies.MaxCount > _post.Likes : true),
                             CanCommand = (_post.CommandPolicies.GroupMemberOnly != null && _post.CommandPolicies.GroupMemberOnly == true ? _chanme.UserId == LoginUserId : true) &&
                             (_post.CommandPolicies.MaxCount != null ? _post.CommandPolicies.MaxCount > _post.Commands : true),
                             CanShare = (_post.SharePolicies.GroupMemberOnly != null && _post.SharePolicies.GroupMemberOnly == true ? _chanme.UserId == LoginUserId : true) &&
                             (_post.SharePolicies.MaxCount != null ? _post.SharePolicies.MaxCount > _post.Shares : true),
                             CanEdit = _creator.UserId == LoginUserId,
                             ImageResponse = postImages.Select(img => new PostImageResponse
                             {
                                 ImageIdval = Encryption.EncryptID(img.PostId.ToString(), LoginUserId.ToString()),
                                 ImageUrl = img.Url,
                                 Description = img.Description
                             }).ToList()
                         })
                         .ToList();
            List<DashboardPostsResponse> dashPost = new List<DashboardPostsResponse>();
            foreach (var item in query)
            {
                List<PostTagResponse> postTags = await (from _potag in _db.PostTags
                                                        join _evtag in _db.EventTags on _potag.EventTagId equals _evtag.EventTagId
                                                        where _potag.PostId == item.PostId
                                                        select new PostTagResponse
                                                        {
                                                            PostTagIdval = Encryption.EncryptID(_potag.PostTagId.ToString(), LoginUserId.ToString()),
                                                            TagName = _evtag.TagName
                                                        }).ToListAsync();

                
                var rawData = await (
                    from _pobal in _db.PostBalances
                    join _po in _db.EventMarkBalances on _pobal.MarkId equals _po.MarkId
                    join _mark in _db.Marks on _pobal.MarkId equals _mark.MarkId
                    where _pobal.PostId == item.PostId && _po.EventPostId == item.EventPostId
                    select new
                    {
                        CollectAmount = _pobal.Balance,
                        EventTotalAmount = _po.TotalBalance,
                        IsoCode = _mark.Isocode,
                        MarkName = _mark.MarkName
                    }
                ).ToListAsync();

                List<PostBalanceResponse> postBalances = rawData
                    .GroupBy(x => new { x.IsoCode, x.MarkName })
                    .Select(g => new PostBalanceResponse
                    {
                        IsoCode = g.Key.IsoCode,
                        MarkName = g.Key.MarkName,
                        CollectAmount = g.Sum(x => Globalfunction.StringToDecimal(Encryption.DecryptID(x.CollectAmount, balanceSalt))),
                        EventTotalAmount = Globalfunction.StringToDecimal(Encryption.DecryptID(g.First().EventTotalAmount, balanceSalt))
                    })
                    .ToList();



                /*
                 List<PostBalanceResponse> postBalances = await (from _pobal in _db.PostBalances
                                                               join _po in _db.EventMarkBalances on _pobal.MarkId equals _po.MarkId
                                                               join _mark in _db.Marks on _pobal.MarkId equals _mark.MarkId
                                                               where _pobal.PostId == item.PostId && _po.EventPostId == item.EventPostId
                                                               select new PostBalanceResponse
                                                               {

                                                                   CollectAmount = Globalfunction.StringToDecimal(Encryption.DecryptID(_pobal.Balance, balanceSalt)),
                                                                   EventTotalAmount = Globalfunction.StringToDecimal(Encryption.DecryptID(_po.TotalBalance, balanceSalt)),
                                                                   IsoCode = _mark.Isocode,
                                                                   MarkName = _mark.MarkName
                }).ToListAsync();*/
                DashboardPostsResponse newPost = new DashboardPostsResponse
                {
                    PostIdval = Encryption.EncryptID(item.PostId.ToString(), LoginUserId.ToString()),
                    Content = item.Content,
                    ChannelIdval = item.ChannelIdval,
                    ChannelName = item.ChannelName,
                    EventPostIdval = Encryption.EncryptID(item.EventPostId.ToString(), LoginUserId.ToString()),
                    EventName = item.EventName,
                    CreatorIdval = item.CreatorIdval,
                    CreatorName = item.CreatorName,
                    ModifiedDate = item.ModifiedDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                    CreatedDate = item.CreatedDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                    LikeTotalCount = item.LikeTotalCount,
                    CommandTotalCount = item.CommandTotalCount,
                    ShareTotalCount = item.ShareTotalCount,
                    Selected = item.Selected,
                    CanLike = item.CanLike,
                    CanCommand = item.CanCommand,
                    CanShare = item.CanShare,
                    CanEdit = item.CanEdit,
                    postTagRes = postTags,
                    postBalanceRes = postBalances,
                    ImageResponse = item.ImageResponse
                };
                dashPost.Add(newPost);
            }
            Pagination pagi = RepoFunService.getWithPagination(pageNumber, pageSize, dashPost);
            result = Result<Pagination>.Success(pagi);

        }
        catch (Exception ex)
        {
            result = Result<Pagination>.Error(ex);
        }
        return result;
    }
    public async Task<Result<string>> CreateEventTags(CreateEventTagListPayload payload, int LoginUserId)
    {
        Result<string> result = null;
        try
        {
            int EventPostId = Convert.ToInt32(Encryption.DecryptID(payload.EventPostIdval!, LoginUserId.ToString()));
            var checkChannelMember = await (from _chan in _db.Channels
                                            join _event in _db.Events on _chan.ChannelId equals _event.ChannelId
                                            join _meme in _db.ChannelMemberships on _chan.ChannelId equals _meme.ChannelId
                                            where _event.PostId == EventPostId && _meme.UserId == LoginUserId
                                            select _meme).FirstOrDefaultAsync();
            if (checkChannelMember == null) return Result<string>.Error("Channel Member Only Can create PostTags");
            List<EventTagPayload> eventTags = payload.EventTags;
            foreach (var item in eventTags)
            {
                EventTag newTag = new EventTag
                {
                    TagName = item.EventTagName,
                    TagDescription = item.EventTagDescritpion,
                    EventPostId = EventPostId,
                    CreatorId = LoginUserId,
                    CreateDate = DateTime.UtcNow,
                    Inactive = false
                };
                await _db.EventTags.AddAsync(newTag);
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


    public async Task<Result<List<PostTagDataResponse>>> GetEventTags(string EventPostIdval, int LoginUserId)
    {
        Result<List<PostTagDataResponse>> result;
        try
        {
            int EventPostId = Convert.ToInt32(Encryption.DecryptID(EventPostIdval, LoginUserId.ToString()));
            List<PostTagDataResponse> query = await _db.EventTags
                .Where(x => x.EventPostId == EventPostId)
                .Select(x => new PostTagDataResponse
                {
                    PostTagIdval = Encryption.EncryptID(x.EventTagId.ToString(), LoginUserId.ToString()),
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

    public async Task<Result<Pagination>> GetEachUserPosts(int LoginUserId, GetEachUserPostsPayload payload)
    {
        Result<Pagination> result = null;
        try
        {
            string balanceSalt = _configuration["appSettings:BalanceSalt"] ?? throw new Exception("Invalid Balance Salt");
            int? UserId = null;
            string Status = payload.Status ?? "approved";
            if (!string.IsNullOrEmpty(payload.UserIdval))
            {
                UserId = Convert.ToInt32(Encryption.DecryptID(payload.UserIdval, LoginUserId.ToString()));
            }
            List<dynamic> netResponse = new List<dynamic>();
            DateTime now = DateTime.UtcNow;
            var posts = await (from _post in _db.Posts
                               where _post.Inactive == false
                               orderby _post.ModifiedDate descending
                               select new
                               {
                                   PostId = _post.PostId,
                                   PostType = _post.PostType,
                                   ModifiedDate = _post.ModifiedDate,
                                   CreatedDate = _post.CreatedDate,
                                   ViewPolicies = _db.PostPolicyProperties.Where(p => p.PostId == _post.PostId && p.PolicyId == 1).FirstOrDefault(),
                                   LikePolicies = _db.PostPolicyProperties.Where(p => p.PostId == _post.PostId && p.PolicyId == 2).FirstOrDefault(),
                                   CommandPolicies = _db.PostPolicyProperties.Where(p => p.PostId == _post.PostId && p.PolicyId == 3).FirstOrDefault(),
                                   SharePolicies = _db.PostPolicyProperties.Where(p => p.PostId == _post.PostId && p.PolicyId == 4).FirstOrDefault(),
                                   UserInteractions = _db.UserPostInteractions.Where(p => p.PostId == _post.PostId && p.UserId == LoginUserId).FirstOrDefault(),
                                   Views = _db.PostViewers.Where(p => p.PostId == _post.PostId).Count(),
                                   Likes = _db.Reacts.Where(p => p.PostId == _post.PostId).Count(),
                                   Commands = _db.PostCommands.Where(p => p.PostId == _post.PostId).Count(),
                                   Shares = _db.PostShares.Where(p => p.PostId == _post.PostId).Count(),
                               })
                   .ToListAsync();
            foreach (var apost in posts)
            {
                if (apost.PostType == "eventpost")
                {
                    var eventQuery = await (from _ev in _db.Events
                                            join _cre in _db.Users on _ev.CreatorId equals _cre.UserId
                                            join _sta in _db.StatusTypes on _ev.StatusId equals _sta.StatusId
                                            join _meship in _db.ChannelMemberships on _ev.ChannelId equals _meship.ChannelId
                                            join _meStatus in _db.StatusTypes on _meship.StatusId equals _meStatus.StatusId
                                            join _usertype in _db.UserTypes on _meship.UserTypeId equals _usertype.TypeId
                                            where apost.PostId == _ev.PostId &&
                                                  _sta.StatusName.ToLower() == Status &&
                                                  (UserId != null ? ( _ev.CreatorId == UserId) : ( _ev.CreatorId == LoginUserId)) &&
                                                   (apost.ViewPolicies.GroupMemberOnly != null 
                                                   && apost.ViewPolicies.GroupMemberOnly == true ? 
                                                   (_meship.UserId == LoginUserId 
                                                   && _meStatus.StatusName.ToLower() == "approved") : true)
                                            select new
                                            {
                                                Event = _ev,
                                                Post = apost,
                                                Creator = _cre,
                                                CMemberShip = _meship,
                                                EventMarks = _ev.PostId,
                                                EventAddresses = _ev.PostId,
                                                EventFiles = _ev.PostId
                                            }).FirstOrDefaultAsync();

                    if (eventQuery is not null)
                    {
                        // Fetch EventMarks
                        var eventMarks = (from _eventMark in _db.EventMarkBalances
                                          join _mark in _db.Marks on _eventMark.MarkId equals _mark.MarkId
                                          join _evall in _db.EventAllowedMarks on _eventMark.MarkId equals _evall.MarkId
                                          where eventQuery.Event.PostId == _eventMark.EventPostId
                                          group new { _eventMark, _mark, _evall } by new { _eventMark.EventPostId, _eventMark.MarkId } into groupedMarks
                                          select new
                                          {
                                              EventPostId = groupedMarks.Key.EventPostId,
                                              Mark = new EventMarks
                                              {
                                                  MarkIdval = Encryption.EncryptID(groupedMarks.Key.MarkId.ToString(), LoginUserId.ToString()),
                                                  IsoCode = groupedMarks.First()._mark.Isocode,
                                                  MarkName = groupedMarks.First()._mark.MarkName,
                                                  AllowedMarkName = groupedMarks.Select(g => g._evall.AllowedMarkName).Distinct().FirstOrDefault(),
                                                  TotalBalance = Globalfunction.StringToDecimal(Encryption.DecryptID(groupedMarks.First()._eventMark.TotalBalance.ToString(), balanceSalt)),
                                                  LastBalance = Globalfunction.StringToDecimal(Encryption.DecryptID(groupedMarks.First()._eventMark.LastBalance.ToString(), balanceSalt)),
                                                  TargetBalance = groupedMarks.First()._eventMark.TargetBalance != null
                                                      ? Globalfunction.StringToDecimal(Encryption.DecryptID(groupedMarks.First()._eventMark.TargetBalance.ToString(), balanceSalt))
                                                      : null
                                              }
                                          }).ToList();


                        // Fetch EventAddresses
                        var eventAddresses = (from _add in _db.EventAddresses
                                              join _atype in _db.AddressTypes on _add.AddressId equals _atype.AddressId
                                              where eventQuery.Event.PostId == _add.EventPostId
                                              select new
                                              {
                                                  EventPostId = _add.EventPostId,
                                                  AddressResponse = new EventAddressResponse
                                                  {
                                                      EventAddressIdval = Encryption.EncryptID(_add.EventAddressId.ToString(), LoginUserId.ToString()),
                                                      Address = _add.AddressName,
                                                      AddresstypeName = _atype.Address
                                                  }
                                              }).ToList();

                        // Fetch EventFiles
                        var eventFiles = _db.EventFiles
                                            .Where(x => eventQuery.Event.PostId == x.EventPostId)
                                            .Select(x => new
                                            {
                                                EventPostId = x.EventPostId,
                                                FileInfo = new EventFileInfo
                                                {
                                                    fileIdval = Encryption.EncryptID(x.UrlId.ToString(), LoginUserId.ToString()),
                                                    imgfilename = x.Url,
                                                    imgDescription = x.UrlDescription
                                                }
                                            }).ToList();

                        bool IsMember = await _db.ChannelMemberships.Where(x => x.ChannelId == eventQuery.Event.ChannelId
                        && x.UserId == LoginUserId).FirstOrDefaultAsync() != null;
                        var finalResult = new DashboardEventPostResponse
                        {
                            PostType = eventQuery.Post.PostType,
                            ChannelIdval = Encryption.EncryptID(eventQuery.Event.ChannelId.ToString(), LoginUserId.ToString()),
                            EventPostIdval = Encryption.EncryptID(eventQuery.Event.PostId.ToString(), LoginUserId.ToString()),
                            EventName = eventQuery.Event.EventName,
                            EventDescrition = eventQuery.Event.EventDescription,
                            CreatorIdval = Encryption.EncryptID(eventQuery.Creator.UserId.ToString(), LoginUserId.ToString()),
                            CreatorName = eventQuery.Creator.Name,
                            StartDate = eventQuery.Event.StartDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                            EndDate = eventQuery.Event.EndDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                            //ModifiedDate = e.Post.ModifiedDate,//.ToString("yyyy-MM-ddTHH:mm:ss")
                            ModifiedDate = apost.ModifiedDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                            CreatedDate = apost.CreatedDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                            ViewTotalCount = apost.Views,
                            LikeTotalCount = apost.Likes,
                            CommandTotalCount = apost.Commands,
                            ShareTotalCount = apost.Shares,
                            IsMember = IsMember,
                            Selected = (_db.Reacts.Where(x => x.UserId == LoginUserId && apost.PostId == x.PostId).FirstOrDefault() != null ? true : false),
                            CanLike = (apost.LikePolicies.GroupMemberOnly != null && apost.LikePolicies.GroupMemberOnly == true ? eventQuery.CMemberShip.UserId == LoginUserId : true) &&
                                          (apost.LikePolicies.MaxCount != null ? apost.LikePolicies.MaxCount > apost.Likes : true),
                            CanCommand = (apost.CommandPolicies.GroupMemberOnly != null && apost.CommandPolicies.GroupMemberOnly == true ? eventQuery.CMemberShip.UserId == LoginUserId : true) &&
                                         (apost.CommandPolicies.MaxCount != null ? apost.CommandPolicies.MaxCount > apost.Commands : true),
                            CanShare = (apost.SharePolicies.GroupMemberOnly != null && apost.SharePolicies.GroupMemberOnly == true ? eventQuery.CMemberShip.UserId == LoginUserId : true) &&
                                         (apost.SharePolicies.MaxCount != null ? apost.SharePolicies.MaxCount > apost.Shares : true),

                            CanEdit = eventQuery.Creator.UserId == LoginUserId,
                            // EventMarks
                            EventMarks = eventMarks.Where(m => m.EventPostId == eventQuery.Event.PostId).Select(m => m.Mark).ToList(),

                            // AddressResponse
                            AddressResponse = eventAddresses.Where(a => a.EventPostId == eventQuery.Event.PostId).Select(a => a.AddressResponse).ToList(),

                            // EventImageList
                            EventImageList = eventFiles.Where(f => f.EventPostId == eventQuery.Event.PostId).Select(f => f.FileInfo).ToList()

                        };
                        netResponse.Add(finalResult);
                    }
                }
                else if (apost.PostType == "collectPost")
                {
                    var query = (from _coll in _db.CollectPosts
                                 join _postStatus in _db.StatusTypes on _coll.StatusId equals _postStatus.StatusId
                                 join _event in _db.Events on _coll.EventPostId equals _event.PostId
                                 join _channel in _db.Channels on _event.ChannelId equals _channel.ChannelId
                                 join _chanme in _db.ChannelMemberships on _event.ChannelId equals _chanme.ChannelId
                                 join _mestatus in _db.StatusTypes on _chanme.StatusId equals _mestatus.StatusId
                                 join _creator in _db.Users on _coll.CreatorId equals _creator.UserId
                                 join _poimg in _db.PostImages on _coll.PostId equals _poimg.PostId into postImages
                                 where _postStatus.StatusName.ToLower() == Status && _coll.PostId == apost.PostId &&
                                 (UserId != null ? ( _coll.CreatorId == UserId) : ( _coll.CreatorId == LoginUserId)) 
                                  && (apost.ViewPolicies.GroupMemberOnly != null 
                                  && apost.ViewPolicies.GroupMemberOnly == true ? 
                                  (_chanme.UserId == LoginUserId && _mestatus.StatusName.ToLower() == "approved") : true)
                                 select new
                                 {
                                     PostId = apost.PostId,
                                     PostType = apost.PostType,
                                     Content = _coll.Content,
                                     ChannelIdval = Encryption.EncryptID(_channel.ChannelId.ToString(), LoginUserId.ToString()),
                                     ChannelName = _channel.ChannelName,
                                     EventPostId = _event.PostId,
                                     EventName = _event.EventName,
                                     CreatorIdval = Encryption.EncryptID(_creator.UserId.ToString(), LoginUserId.ToString()),
                                     CreatorName = _creator.Name,
                                     ModifiedDate = apost.ModifiedDate,
                                     CreatedDate = apost.CreatedDate,
                                     ViewTotalCount = apost.Views,
                                     LikeTotalCount = apost.Likes,
                                     CommandTotalCount = apost.Commands,
                                     ShareTotalCount = apost.Shares,
                                     Selected = (_db.Reacts.Where(x => x.UserId == LoginUserId && apost.PostId == x.PostId).FirstOrDefault() != null ? true : false),
                                     CanLike = (apost.LikePolicies.GroupMemberOnly != null && apost.LikePolicies.GroupMemberOnly == true ? _chanme.UserId == LoginUserId : true) &&
                                      (apost.LikePolicies.MaxCount != null ? apost.LikePolicies.MaxCount > apost.Likes : true),
                                     CanCommand = (apost.CommandPolicies.GroupMemberOnly != null && apost.CommandPolicies.GroupMemberOnly == true ? _chanme.UserId == LoginUserId : true) &&
                                     (apost.CommandPolicies.MaxCount != null ? apost.CommandPolicies.MaxCount > apost.Commands : true),
                                     CanShare = (apost.SharePolicies.GroupMemberOnly != null && apost.SharePolicies.GroupMemberOnly == true ? _chanme.UserId == LoginUserId : true) &&
                                     (apost.SharePolicies.MaxCount != null ? apost.SharePolicies.MaxCount > apost.Shares : true),
                                     CanEdit = _creator.UserId == LoginUserId,
                                     ImageResponse = postImages.Select(img => new PostImageResponse
                                     {
                                         ImageIdval = Encryption.EncryptID(img.PostId.ToString(), LoginUserId.ToString()),
                                         ImageUrl = img.Url,
                                         Description = img.Description
                                     }).ToList()
                                 })
                               .ToList();
                    foreach (var item in query)
                    {
                        List<PostTagResponse> postTags = await (from _potag in _db.PostTags
                                                                join _evtag in _db.EventTags on _potag.EventTagId equals _evtag.EventTagId
                                                                where _potag.PostId == item.PostId
                                                                select new PostTagResponse
                                                                {
                                                                    PostTagIdval = Encryption.EncryptID(_potag.PostTagId.ToString(), LoginUserId.ToString()),
                                                                    TagName = _evtag.TagName
                                                                }).ToListAsync();
                        List<PostBalanceResponse> postBalances = await (from _pobal in _db.PostBalances
                                                                        //join _allow in _db.EventAllowedMarks on _pobal.MarkId equals _allow.MarkId
                                                                        join _po in _db.EventMarkBalances on _pobal.MarkId equals _po.MarkId
                                                                        join _mark in _db.Marks on _pobal.MarkId equals _mark.MarkId
                                                                        where _pobal.PostId == item.PostId && _po.EventPostId == item.EventPostId
                                                                        select new PostBalanceResponse
                                                                        {
                                                                            CollectAmount = Globalfunction.StringToDecimal(Encryption.DecryptID(_pobal.Balance, balanceSalt)),
                                                                            EventTotalAmount = Status.ToLower()=="pending" ?
                                                                            Globalfunction.StringToDecimal(Encryption.DecryptID(_pobal.Balance, balanceSalt)) + Globalfunction.StringToDecimal(Encryption.DecryptID(_po.TotalBalance, balanceSalt))
                                                                            : Globalfunction.StringToDecimal(Encryption.DecryptID(_po.TotalBalance, balanceSalt)),
                                                                            IsoCode = _mark.Isocode,
                                                                            MarkName = _mark.MarkName
                                                                        }).ToListAsync();
                        DashboardPostsResponse newPost = new DashboardPostsResponse
                        {
                            PostType = item.PostType,
                            PostIdval = Encryption.EncryptID(item.PostId.ToString(), LoginUserId.ToString()),
                            Content = item.Content,
                            ChannelIdval = item.ChannelIdval,
                            ChannelName = item.ChannelName,
                            EventPostIdval = Encryption.EncryptID(item.EventPostId.ToString(), LoginUserId.ToString()),
                            EventName = item.EventName,
                            CreatorIdval = item.CreatorIdval,
                            CreatorName = item.CreatorName,
                            ModifiedDate = item.ModifiedDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                            CreatedDate = item.CreatedDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                            LikeTotalCount = item.LikeTotalCount,
                            CommandTotalCount = item.CommandTotalCount,
                            ShareTotalCount = item.ShareTotalCount,
                            Selected = item.Selected,
                            CanLike = item.CanLike,
                            CanCommand = item.CanCommand,
                            CanShare = item.CanShare,
                            CanEdit = item.CanEdit,
                            postTagRes = postTags,
                            postBalanceRes = postBalances,
                            ImageResponse = item.ImageResponse
                        };
                        netResponse.Add(newPost);
                    }
                }
            }
            Pagination pagi = RepoFunService.getWithPagination(payload.pageNumber, payload.pageSize, netResponse);
            result = Result<Pagination>.Success(pagi);

        }
        catch (Exception ex)
        {
            result = Result<Pagination>.Error(ex);
        }
        return result;
    }

    public async Task<Result<string>> DeletePost(int LoginUserId, string postIdval)
    {
        Result<string> result = null;
        try
        {
            int PostId = Convert.ToInt32(Encryption.DecryptID(postIdval, LoginUserId.ToString()));
            var post = await _db.Posts.Where(x => x.PostId == PostId).FirstOrDefaultAsync();
            if (post is null)
                return Result<string>.Error("Post Not Found");

            if(post.PostType.ToLower() == "eventpost")
            {
                string? statusName = await (from _ev in _db.Events
                                            join _status in _db.StatusTypes on _ev.StatusId equals _status.StatusId
                                            where _ev.PostId == PostId
                                            select _status.StatusName).FirstOrDefaultAsync();
                if(statusName is not null)
                {
                    if (statusName.ToLower() == "approved")
                        return Result<string>.Error("You can't delete this post because this has been approved by admin");

                    post.Inactive = true;
                    await _db.SaveChangesAsync();
                }
            }else if(post.PostType.ToLower() == "collectpost")
            {
                string? statusName = await (from _coll in _db.CollectPosts
                                            join _status in _db.StatusTypes on _coll.StatusId equals _status.StatusId
                                            where _coll.PostId == PostId
                                            select _status.StatusName).FirstOrDefaultAsync();
                if (statusName is not null)
                {
                    if (statusName.ToLower() == "approved")
                        return Result<string>.Error("You can't delete this post because this has been approved by admin");

                    post.Inactive = true;
                    await _db.SaveChangesAsync();
                }
            }
            else
            {
                return Result<string>.Error("Invalid Post Type");
            }
            result = Result<string>.Success("Move to bin successfully");
        }
        catch (Exception ex)
        {
            result = Result<string>.Error(ex);
        }
        return result;
    }
}
