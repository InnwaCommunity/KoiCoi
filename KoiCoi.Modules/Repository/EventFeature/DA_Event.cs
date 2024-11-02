using Amazon;
using Amazon.S3.Model;
using Humanizer;
using KoiCoi.Database.AppDbContextModels;
using KoiCoi.Models.EventDto.Payload;
using KoiCoi.Models.EventDto.Response;
using KoiCoi.Modules.Repository.ChangePassword;
using KoiCoi.Modules.Repository.UserFeature;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Utilities.Encoders;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Globalization;
using System.Text.RegularExpressions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace KoiCoi.Modules.Repository.EventFreture;

public class DA_Event
{
    private readonly AppDbContext _db;
    private readonly NotificationManager.NotificationManager _saveNotifications;
    private readonly IConfiguration _configuration;
    private readonly KcAwsS3Service _kcAwsS3Service;

    public DA_Event(AppDbContext db, 
        IConfiguration configuration, 
        NotificationManager.NotificationManager saveNotifications,
        KcAwsS3Service kcAwsS3Service)
    {
        _db = db;
        _configuration = configuration;
        _saveNotifications = saveNotifications;
        _kcAwsS3Service = kcAwsS3Service;
    }

    public async Task<Result<string>> CreateEvent(CreateEventPayload paylod,int LoginUserId)
    {
        Result<string> result = null;
        try
        {
            string balanceSalt = _configuration["appSettings:BalanceSalt"] ?? throw new Exception("Invalid Balance Salt");
            int ChannelId = Convert.ToInt32(Encryption.DecryptID(paylod.ChannelIdval!, LoginUserId.ToString()));
            int status = 0;
            var ownerusertype = await (from _me in _db.ChannelMemberships
                                       join _uset in _db.UserTypes on _me.UserTypeId equals _uset.TypeId
                                       where _me.ChannelId == ChannelId && _me.UserId == LoginUserId &&
                                       _uset.Name.ToLower() == "owner" 
                                       select new
                                       {
                                           UserId = _me.UserId,
                                       })
                                       .FirstOrDefaultAsync();
            if(ownerusertype is not null)
            {
                status = await _db.StatusTypes.Where(x => x.StatusName.ToLower() == "approved")
                    .Select(x => x.StatusId)
                    .FirstOrDefaultAsync();
            }
            else
            {
                status = await _db.StatusTypes.Where(x => x.StatusName.ToLower() == "pending")
                    .Select(x => x.StatusId)
                    .FirstOrDefaultAsync();
            }
            if (status is 0) return Result<string>.Error("Pending Status Not Found");
            var neweventPost = new Post
            {
                PostType = "eventpost",
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow,
                Inactive = false
            };
            await _db.Posts.AddAsync(neweventPost);
            await _db.SaveChangesAsync();
            Event newEvent = new Event
            {
                EventName = paylod.EventName!,
                PostId = neweventPost.PostId,
                EventDescription = paylod.EventDescription,
                ChannelId = ChannelId,
                CreatorId = LoginUserId,
                ApproverId = ownerusertype is not null ? LoginUserId : null,
                StatusId = status,
                StartDate = DateTime.Parse(paylod!.StartDate!),
                EndDate = DateTime.Parse(paylod!.EndDate!)
            };
            var res = await _db.Events.AddAsync(newEvent);
            await _db.SaveChangesAsync();
            string PostIdval = Encryption.EncryptID(neweventPost.PostId.ToString(), LoginUserId.ToString());
            
            result = Result<string>.Success(PostIdval);
            ///Save Policies
            await SavePostPolicies(neweventPost.PostId, 1, paylod.viewPolicy);//Save View Policy
            await SavePostPolicies(neweventPost.PostId, 2, paylod.reactPolicy);//Save React Policy
            await SavePostPolicies(neweventPost.PostId, 3, paylod.commandPolicy);//Save Command Policy
            await SavePostPolicies(neweventPost.PostId, 4, paylod.sharePolicy);//Save Share Policy
            if (paylod.EventAddresses.Any())
            {
                foreach (var address in paylod.EventAddresses)
                {
                    int AddressId = Convert.ToInt32(Encryption.DecryptID(address.AddressTypeIdval, LoginUserId.ToString()));
                    EventAddress newAddress = new EventAddress
                    {
                        AddressId = AddressId,
                        EventPostId = newEvent.PostId,
                        AddressName = address.AddressName,
                    };
                    await _db.EventAddresses.AddAsync(newAddress);
                    await _db.SaveChangesAsync();
                }
            }
            if(ownerusertype is not null)
            {
                var ownertype = await _db.UserTypes
                                      .Where(x => x.Name.ToLower() == "owner")
                                       .FirstOrDefaultAsync();
                if (ownertype is not null)
                {
                    EventMembership newme = new EventMembership
                    {
                        EventPostId = neweventPost.PostId,
                        UserId = LoginUserId,
                        UserTypeId = ownertype.TypeId,
                    };
                    await _db.EventMemberships.AddAsync(newme);
                    await _db.SaveChangesAsync();
                }
            }
            /*if(paylod.EventPhotos.Any())
            {
                string baseDirectory = _configuration["appSettings:UploadPath"] ?? throw new Exception("Invalid UploadPath");
                string uploadDirectory = _configuration["appSettings:EventImages"] ?? throw new Exception("Invalid function upload path.");
                string destDirectory = Path.Combine(baseDirectory, uploadDirectory);
                if (!Directory.Exists(destDirectory))
                {
                    Directory.CreateDirectory(destDirectory);
                }

                string filename = Globalfunction.NewUniqueFileName() + ".png";
                string base64Str = item.base64image!;
                byte[] bytes = Convert.FromBase64String(base64Str!);

                string filePath = Path.Combine(destDirectory, filename);
                if (filePath.Contains(".."))
                { //if found .. in the file name or path
                    Log.Error("Invalid path " + filePath);
                }
                await System.IO.File.WriteAllBytesAsync(filePath, bytes);
                 

            string bucketname = _configuration.GetSection("Buckets:EventImages").Get<string>()!;

            foreach (var item in paylod.EventPhotos)
            {
                string uniquekey = Globalfunction.NewUniqueFileKey(item.ext!);
                await _kcAwsS3Service.CreateFileAsync(item.base64image!, bucketname, uniquekey, item.ext!);
                var newImage = new EventFile
                {
                    Url = uniquekey,
                    UrlDescription = item.Description,
                    EventPostId = newEvent.PostId,
                    CreatedDate = DateTime.UtcNow,
                    ModifiedDate = DateTime.UtcNow,
                    Extension = "png",
                };
                await _db.EventFiles.AddAsync(newImage);
                await _db.SaveChangesAsync();
            }
        }
             */
            if (ownerusertype is not null)
            {
                ///Created the Event by owner
                List<int> channelMember = await _db.ChannelMemberships.Where(x=> x.ChannelId == ChannelId)
                    .Select(x=> x.UserId).ToListAsync();
                if (channelMember.Contains(LoginUserId))
                {
                    channelMember.Remove(LoginUserId);
                }
                await _saveNotifications.SaveNotification(channelMember,
                    LoginUserId,
                    $"Upcoming the New Event {newEvent.EventName}",
                    newEvent.EventDescription,
                    $"UpcomingNewEvent/{newEvent.PostId}");
            }
            else
            {
                ///Pending the Event 
                List<int> admins = await (from _meme in _db.ChannelMemberships
                                          join _usertype in _db.UserTypes on _meme.UserId equals _usertype.TypeId
                                          where _meme.ChannelId == ChannelId &&
                                          (_usertype.Name.ToLower() == "owner" || _usertype.Name.ToLower() == "admin")
                                          select _meme.UserId).ToListAsync();
                if (admins.Contains(LoginUserId))
                {
                    admins.Remove(LoginUserId);
                }
                string? LoginUserName= await _db.Users.Where(x=> x.UserId == LoginUserId)
                    .Select(x=> x.Name).FirstOrDefaultAsync();
                await _saveNotifications.SaveNotification(admins,
                    LoginUserId,
                    $"Requested the New Event {newEvent.EventName} by Member {LoginUserName}",
                    newEvent.EventDescription,
                    $"RequestedNewEvent/{newEvent.PostId}");
            }
        }
        catch (Exception ex)
        {
            result = Result<string>.Error(ex);
        }

        return result;
    }

    public async Task<Result<string>> UploadEventAttachFile(IFormFile file, string eventPostIdval, int LoginUserId)
    {
        Result<string> result = null;
        try
        {
                int PostId = Convert.ToInt32(Encryption.DecryptID(eventPostIdval, LoginUserId.ToString()));
                var kcevent = await _db.Posts.Where(x=> x.PostId == PostId).FirstOrDefaultAsync();
                if (kcevent is not null)
                {
                       string bucketname = _configuration.GetSection("Buckets:EventImages").Get<string>()!;

                        string ext = Path.GetExtension(file.FileName);
                        string uniquekey = Globalfunction.NewUniqueFileKey(ext);
                        Result<string> res= await _kcAwsS3Service.CreateFileAsync(file, bucketname, uniquekey, ext);
                        if (res.IsSuccess)
                        {
                            var newImage = new EventFile
                            {
                                Url = uniquekey,
                                UrlDescription = "",
                                EventPostId = kcevent.PostId,
                                CreatedDate = DateTime.UtcNow,
                                ModifiedDate = DateTime.UtcNow,
                                Extension = ext,
                            };
                            await _db.EventFiles.AddAsync(newImage);
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
                    result = Result<string>.Error("Event Not Found");
                }
        }
        catch (Exception ex)
        {
            result = Result<string>.Error(ex);
        }
        return result;
    }
    private async Task SavePostPolicies(int postid,int policyId, PostPolicyPropertyPayload policy)
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

    public async Task<Result<Pagination>> GetEventRequestList(GetEventRequestPayload payload, int LoginUserId)
    {
        Result<Pagination> result;
        try
        {
            int ChannelId = Convert.ToInt32(Encryption.DecryptID(payload.ChannelIdval!, LoginUserId.ToString()));
            string balanceSalt = _configuration["appSettings:BalanceSalt"] ?? throw new Exception("Invalid Balance Salt");
            string status = payload.Status!;
            var query = await (from _ev in _db.Events
                               join _post in _db.Posts on _ev.PostId equals _post.PostId
                               join _cre in _db.Users on _ev.CreatorId equals _cre.UserId
                               join _sta in _db.StatusTypes on _ev.StatusId equals _sta.StatusId
                               join _meship in _db.ChannelMemberships on _ev.ChannelId equals _meship.ChannelId
                               join _usertype in _db.UserTypes on _meship.UserTypeId equals _usertype.TypeId
                               where _ev.ChannelId == ChannelId &&
                               _post.PostType == "eventpost"
                               && _sta.StatusName.ToLower() == status
                               && _post.Inactive == false
                               && _meship.UserId == LoginUserId
                               && (status.ToLower() == "approved" ||
                               _usertype.Name.ToLower() == "owner" ||
                               _usertype.Name.ToLower() == "admin")
                               select new GetRequestEventResponse
                               {
                                   EventPostIdval = Encryption.EncryptID(_ev.PostId.ToString(), LoginUserId.ToString()),
                                   EventName = _ev.EventName,
                                   EventDescrition = _ev.EventDescription,
                                   CreatorIdval = Encryption.EncryptID(_cre.UserId.ToString(), LoginUserId.ToString()),
                                   CreatorName = _cre.Name,
                                   //TotalBalance = Globalfunction.StringToDecimal(Encryption.DecryptID(_ev.TotalBalance.ToString(), balanceSalt)),
                                   //LastBalance = Globalfunction.StringToDecimal(Encryption.DecryptID(_ev.LastBalance.ToString(), balanceSalt)),
                                   //TargetBalance = !string.IsNullOrEmpty(_ev.TargetBalance) ? Globalfunction.StringToDecimal(Encryption.DecryptID(_ev.TargetBalance!.ToString(), balanceSalt)) : 0,
                                   StartDate = _ev.StartDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                                   EndDate = _ev.EndDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                                   ModifiedDate = _post.ModifiedDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                                   EventMarks = (from _eventMark in _db.EventMarkBalances
                                                 join _mark in _db.Marks on _eventMark.MarkId equals _mark.MarkId
                                                 where _eventMark.EventPostId == _ev.PostId
                                                 select new EventMarks
                                                 {
                                                     MarkIdval = Encryption.EncryptID(_eventMark.MarkId.ToString(), LoginUserId.ToString()),
                                                     IsoCode = _mark.Isocode,
                                                     TotalBalance = Globalfunction.StringToDecimal(Encryption.DecryptID(_eventMark.TotalBalance.ToString(), balanceSalt)),
                                                     LastBalance = Globalfunction.StringToDecimal(Encryption.DecryptID(_eventMark.LastBalance.ToString(), balanceSalt)),
                                                     TargetBalance = _eventMark.TargetBalance != null ? Globalfunction.StringToDecimal(Encryption.DecryptID(_eventMark.TargetBalance.ToString(), balanceSalt)) : null,

                                                 }).ToList(),
                                   AddressResponse = (from _add in _db.EventAddresses
                                                      join _atype in _db.AddressTypes on _add.AddressId equals _atype.AddressId
                                                      where _add.EventPostId == _ev.PostId
                                                      select new EventAddressResponse
                                                      {
                                                          Address = _add.AddressName,
                                                          AddresstypeName = _atype.Address
                                                      }).ToList(),
                                   EventImageList = _db.EventFiles.Where(x => x.EventPostId == _ev.PostId)
                                                        .Select(x => new EventFileInfo
                                                        {
                                                            fileIdval = Encryption.EncryptID(x.UrlId.ToString(), LoginUserId.ToString()),
                                                            imgfilename = x.Url,
                                                            imgDescription = x.UrlDescription
                                                        }).ToList(),
                               }).ToListAsync();

            Pagination data = RepoFunService.getWithPagination(payload.PageNumber, payload.PageSize, query);
            /*
            List<GetRequestEventResponse> responseList = new List<GetRequestEventResponse>();
            foreach (var item in query)
            {
                GetRequestEventResponse newres= new GetRequestEventResponse
                {
                    EventPostIdval = Encryption.EncryptID(item.EventPostId.ToString(), LoginUserId.ToString()),
                    EventName = item.EventName,
                    EventDescrition = item.EventDescrition,
                    CreatorIdval = Encryption.EncryptID(item.CreatorIdval.ToString(), LoginUserId.ToString()),
                    CreatorName = item.CreatorName,
                    StartDate = item.StartDate,
                    EndDate = item.EndDate,
                    ModifiedDate = item.ModifiedDate,
                    AddressResponse = item.AddressResponse,
                    EventImageList = imgquery
                };
                responseList.Add(newres);
            }
             */
            result = Result<Pagination>.Success(data);
        }
        catch (Exception ex)
        {
            result = Result<Pagination>.Error(ex);
        }

        return result;
    }

    public async Task<Result<string>> ApproveRejectEvent(List<ApproveRejectEventPayload> payload, int LoginUserId)
    {
        Result<string> result = null;
        try
        {
            foreach (var item in payload)
            {
                int EventPostId = Convert.ToInt32(Encryption.DecryptID(item.EventPostIdval!, LoginUserId.ToString()));
                var checkloginusertype = await (from _ev in _db.Events
                                                join _meme in _db.ChannelMemberships on _ev.ChannelId equals _meme.ChannelId
                                                join _usety in _db.UserTypes on _meme.UserTypeId equals _usety.TypeId
                                                where _ev.PostId == EventPostId
                                                && _meme.UserId == LoginUserId
                                                && (_usety.Name.ToLower() == "owner" || _usety.Name.ToLower() == "admin")
                                                select _usety.Name)
                                                .FirstOrDefaultAsync();
                if (checkloginusertype is not null)
                {
                    var oldevent = await _db.Events.Where(x => x.PostId == EventPostId).FirstOrDefaultAsync();
                    if(oldevent is not null)
                    {
                        if (item.Status == 1)
                        {
                            int approvedstatusId = await _db.StatusTypes
                                                    .Where(x => x.StatusName.ToLower() == "approved")
                                                    .Select(x => x.StatusId).FirstOrDefaultAsync();
                            if (approvedstatusId != 0)
                            {
                                oldevent.ApproverId = LoginUserId;
                                oldevent.StatusId = approvedstatusId;
                            }
                            await _db.SaveChangesAsync();
                            ///Save as owner the eventcreator and channel owner
                            var ownerusertype = await _db.UserTypes
                                                    .Where(x => x.Name.ToLower() == "owner")
                                                    .FirstOrDefaultAsync();
                            if (ownerusertype is not null)
                            {
                                EventMembership newme = new EventMembership
                                {
                                    EventPostId = oldevent.PostId,
                                    UserId = oldevent.CreatorId,
                                    UserTypeId = ownerusertype.TypeId,
                                };
                                await _db.EventMemberships.AddAsync(newme);
                                await _db.SaveChangesAsync();
                            }

                            ///Remind to all channel Members
                            List<int> channelMembers = await _db.ChannelMemberships
                                                            .Where(x=> x.ChannelId == oldevent.ChannelId)
                                                            .Select(x=> x.UserId)
                                                            .ToListAsync();
                            
                            await _saveNotifications.SaveNotification(
                                channelMembers,
                                LoginUserId,
                                oldevent.EventName,
                                oldevent.EventDescription,
                                $"UpcomingNewEvent/{oldevent.PostId}");
                        }
                        else if (item.Status == 2)
                        {
                            int approvedstatusId = await _db.StatusTypes
                                                   .Where(x => x.StatusName.ToLower() == "rejected")
                                                   .Select(x => x.StatusId).FirstOrDefaultAsync();
                            if (approvedstatusId != 0)
                            {
                                oldevent.ApproverId = LoginUserId;
                                oldevent.StatusId = approvedstatusId;
                            }
                            await _db.SaveChangesAsync();

                            ///Notifi to admins and event creator
                            List<int> admins = await (from _mem in _db.ChannelMemberships
                                                      join _use in _db.UserTypes on _mem.UserTypeId equals _use.TypeId
                                                      where _mem.ChannelId == oldevent.ChannelId
                                                      && (_use.Name.ToLower() == "owner" || _use.Name.ToLower() == "admin")
                                                      select _mem.UserId).ToListAsync();
                            var loginName = await _db.Users
                                .Where(x => x.UserId == LoginUserId)
                                .Select(x => x.Name).FirstOrDefaultAsync();
                            admins.Add(oldevent.CreatorId);
                            if (admins.Contains(LoginUserId))
                            {
                                admins.Remove(LoginUserId);
                            }
                            await _saveNotifications.SaveNotification(
                                admins,
                                LoginUserId,
                                $"Rejected Event {oldevent.EventName}",
                                $"{loginName} Rejected Event {oldevent.EventName}",
                                $"RejectedNewEvent/{oldevent.PostId}");
                        }

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


    public async Task<Result<string>> ChangeUserTypeTheEventMemberships(ChangeUserTypeEventMembership payload, int LoginUserId)
    {
        Result<string> result = null;
        try
        {
            int EventPostId = Convert.ToInt32(Encryption.DecryptID(payload.EventPostIdval!, LoginUserId.ToString()));
            var checkLoginUserAccess = await (from meship in _db.EventMemberships
                                              join usertype in _db.UserTypes on meship.UserTypeId equals usertype.TypeId
                                              where meship.UserId == LoginUserId && meship.EventPostId == EventPostId
                                             && (usertype.Name.ToLower() == "admin" || usertype.Name.ToLower() == "owner")
                                             select usertype.Name
                                              ).FirstOrDefaultAsync();
            List<UserIdAndUserType> userIdAndUserTypes = payload.userIdAndUserTypes!;
            if (checkLoginUserAccess is not null)
            {
                foreach (var item in userIdAndUserTypes)
                {
                    int UserId = Convert.ToInt32(Encryption.DecryptID(item.UserIdval!, LoginUserId.ToString()));
                    int UserTypeId = Convert.ToInt32(Encryption.DecryptID(item.UserTypeIdval!, LoginUserId.ToString()));
                    var eventme = await _db.EventMemberships
                        .Where(x => x.EventPostId == EventPostId && x.UserId == UserId)
                        .FirstOrDefaultAsync();
                    if (eventme is null)
                    {
                        eventme = new EventMembership
                        {
                            EventPostId = EventPostId,
                            UserId = UserId,
                            UserTypeId = UserTypeId
                        };
                        await _db.EventMemberships.AddAsync(eventme);
                        await _db.SaveChangesAsync();
                    }
                    else
                    {
                        eventme.UserTypeId = UserTypeId;
                        await _db.SaveChangesAsync();
                    }


                    ///Save Noti to admins
                    var admins = await (from _meme in _db.EventMemberships
                                        join _ut in _db.UserTypes on _meme.UserTypeId equals _ut.TypeId
                                        where _meme.EventPostId == EventPostId
                                        && (_ut.Name.ToLower() == "owner" || _ut.Name.ToLower() == "admin")
                                        select _meme.UserId).ToListAsync();
                    admins.Add(UserId);
                    if (admins.Contains(LoginUserId))
                    {
                        admins.Remove(LoginUserId);
                    }
                    admins.Distinct();
                    var data = await (from _meme in _db.EventMemberships
                                      join _user in _db.Users on _meme.UserId equals _user.UserId
                                      join _ut in _db.UserTypes on _meme.UserTypeId equals _ut.TypeId
                                      where _meme.EventPostId == EventPostId
                                      && _meme.UserId == UserId
                                      select new
                                      {
                                          UserName = _user.Name,
                                          UserType = _ut.Name
                                      }).FirstOrDefaultAsync();
                    string? loginname = _db.Users.Where(x => x.UserId == LoginUserId).Select(x => x.Name).FirstOrDefault();
                    if (data is not null && loginname is not null)
                    {
                        await _saveNotifications.SaveNotification(admins,
                            LoginUserId,
                            $"Changed the UserType of {data.UserName}",
                            $"{loginname} Changed {data.UserName} to {data.UserType}",
                            $"EventUserTypeChange/{eventme.Membershipid}");
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

    public async Task<Result<List<EventAdminsResponse>>> GetEventOwnerAndAdmins(GetEventDataPayload payload, int LoginUserId)
    {
        Result<List<EventAdminsResponse>> result = null;
        try
        {
            int EventPostId = Convert.ToInt32(Encryption.DecryptID(payload.EventPostIdval!, LoginUserId.ToString()));
            List<EventAdminsResponse> query = await (from _em in _db.EventMemberships
                               join _ut in _db.UserTypes on _em.UserTypeId equals _ut.TypeId
                               join _meb in _db.Users on _em.UserId equals _meb.UserId
                               join _ev in _db.Events on _em.EventPostId equals _ev.PostId
                               join _ms in _db.ChannelMemberships on _ev.ChannelId equals _ms.ChannelId
                               join _logu in _db.Users on _ms.UserId equals _logu.UserId
                               where _em.EventPostId == EventPostId && _logu.UserId == LoginUserId
                               && (_ut.Name.ToLower() == "owner" || _ut.Name.ToLower() == "admin")
                               select new EventAdminsResponse
                               {
                                   AdminIdval = Encryption.EncryptID(_meb.UserId.ToString(), LoginUserId.ToString()),
                                   AdminName = _meb.Name,
                                   UserTypes = _ut.Name
                               }).ToListAsync();
            result = Result<List<EventAdminsResponse>>.Success(query);
        }
        catch (Exception ex)
        {
            result = Result<List<EventAdminsResponse>>.Error(ex);
        }
        return result;
    }

    public async Task<Result<Pagination>> GetAddressTypes(int LoginUserID,int PageNumber,int PageSize)
    {
        Result<Pagination> result = null;
        try
        {
            List<AddressTypeResponse> query = await _db.AddressTypes
                .Select(x => new AddressTypeResponse
                {
                    AddressTypeIdval = Encryption.EncryptID(x.AddressId.ToString(),LoginUserID.ToString()),
                    Address = x.Address,
                    Description = x.Description
                }).ToListAsync();
            Pagination data = RepoFunService.getWithPagination(PageNumber, PageSize, query);
            result = Result<Pagination>.Success(data);
        }
        catch (Exception ex)
        {
            result = Result<Pagination>.Error(ex);
        }
        return result;
    }
    public async Task<Result<string>> EditStartDateandEndDate(EditStardEndDate payload, int LoginUserID)
    {
        Result<string> result = null;
        try
        {
            if (payload.EventPostIdval is not null && payload.StardDate is not null && payload.EndDate is not null)
            {
                int EventPostId = Convert.ToInt32(Encryption.DecryptID(payload.EventPostIdval, LoginUserID.ToString()));
                var post = await _db.Posts.Where(x => x.PostId == EventPostId).FirstOrDefaultAsync();
                if (post is not null)
                {

                    var owner = await (from _p in _db.Posts
                                       join _ms in _db.EventMemberships on _p.PostId equals _ms.EventPostId
                                       join _ut in _db.UserTypes on _ms.UserTypeId equals _ut.TypeId
                                       where _p.PostId == EventPostId && _ms.UserId == LoginUserID && (_ut.Name.ToLower() == "owner" || _ut.Name.ToLower() == "admin")
                                       select _ms).FirstOrDefaultAsync();
                    ///Check Owner or Admin
                    if(owner is not null)
                    {
                        var pevent = await _db.Events.Where(x => x.PostId == post.PostId).FirstOrDefaultAsync();
                        if (pevent is not null)
                        {
                            ///Check Collect Posts that have been upload in this event
                            var cpost = await _db.CollectPosts.Where(x => x.EventPostId == EventPostId).FirstOrDefaultAsync();
                            if (cpost is null)
                            {
                                pevent.StartDate = DateTime.Parse(payload.StardDate);
                                pevent.EndDate = DateTime.Parse(payload.EndDate);
                                post.ModifiedDate = DateTime.UtcNow;
                                await _db.SaveChangesAsync();
                                result = Result<string>.Success("Success");
                            }
                            else
                            {
                                ///Don't Allow to edit Stard Date because Some Collect Posts Have Been Posted.
                                pevent.EndDate = DateTime.Parse(payload.EndDate);
                                post.ModifiedDate = DateTime.UtcNow;
                                await _db.SaveChangesAsync();
                                result = Result<string>.Success("Success,But can't edit start date because some collect post have been posted");
                            }
                        }
                        else
                        {
                            result = Result<string>.Error("Event Not Found");
                        }
                    }
                    else
                    {
                        result = Result<string>.Error("Can't Access To Edit");
                    }
                }
                else
                {
                    result = Result<string>.Error("Event Post Not Found");
                }
            }
            else
            {
                result = Result<string>.Error("EventPostId can't null");
            }
        }
        catch (Exception ex)
        {
            result = Result<string>.Error(ex);
        }
        return result;
    }

    public async Task<Result<Pagination>> GetEventByStatusAndDate(OrderByMonthPayload payload, int LoginUserId)
    {
        //List<GetRequestEventResponse>
        Result<Pagination> result = null;
        try
        {
            if (payload.Status is null || payload is null)
                return Result<Pagination>.Error("Please add Stauts");

            if (payload.Status.ToLower() != "active" && payload.Status.ToLower() != "last" && payload.Status.ToLower() != "upcoming")
                return Result<Pagination>.Error("Invalide Status");

            if(payload.Month is not null && payload.Idval is not null && payload.PageNumber >= 1 && payload.PageSize >= 1)
            {
                CultureInfo provider = CultureInfo.InvariantCulture;
                DateTime dateTime = DateTime.ParseExact(payload.Month, "yyyy-MM-dd", provider);
                int ChannelId = Convert.ToInt32(Encryption.DecryptID(payload.Idval!, LoginUserId.ToString()));
                string status = payload.Status;
                DateTime now = DateTime.UtcNow;
                //bool currentDate = dateTime.Year == now.Year && dateTime.Month == now.Month;
                string balanceSalt = _configuration["appSettings:BalanceSalt"] ?? throw new Exception("Invalid Balance Salt");

                /*var query = await (from _ev in _db.Events
                                   join _post in _db.Posts on _ev.PostId equals _post.PostId
                                   join _cre in _db.Users on _ev.CreatorId equals _cre.UserId
                                   join _sta in _db.StatusTypes on _ev.StatusId equals _sta.StatusId
                                   join _meship in _db.ChannelMemberships on _ev.ChannelId equals _meship.ChannelId
                                   join _usertype in _db.UserTypes on _meship.UserTypeId equals _usertype.TypeId
                                   where _ev.ChannelId == ChannelId &&
                                   _post.PostType == "eventpost"
                                   && _sta.StatusName.ToLower() == "approved"
                                   && _post.Inactive == false
                                   && _meship.UserId == LoginUserId && (
                                   status=="active" ? 
                                   _ev.StartDate <= dateTime && 
                                   _ev.EndDate >= dateTime : 
                                   status == "last" ?  
                                   _ev.EndDate.Year == dateTime.Year &&
                                   _ev.EndDate.Month == dateTime.Month &&
                                   _ev.EndDate.Day == dateTime.Day :
                                   status == "upcoming" ?
                                   _ev.StartDate.Year == dateTime.Year &&
                                   _ev.StartDate.Month == dateTime.Month &&
                                   _ev.StartDate.Day == dateTime.Day : false )
                                   select new GetRequestEventResponse
                                   {
                                       EventPostIdval = Encryption.EncryptID(_ev.PostId.ToString(), LoginUserId.ToString()),
                                       EventName = _ev.EventName,
                                       EventDescrition = _ev.EventDescription,
                                       CreatorIdval = Encryption.EncryptID(_cre.UserId.ToString(), LoginUserId.ToString()),
                                       CreatorName = _cre.Name,
                                       StartDate = _ev.StartDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                                       EndDate = _ev.EndDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                                       ModifiedDate = _post.ModifiedDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                                       EventMarks = (from _eventMark in _db.EventMarkBalances
                                                     join _mark in _db.Marks on _eventMark.MarkId equals _mark.MarkId
                                                     where _eventMark.EventPostId == _ev.PostId
                                                     select new EventMarks
                                                     {
                                                         MarkIdval=Encryption.EncryptID(_eventMark.MarkId.ToString(),LoginUserId.ToString()),
                                                         IsoCode = _mark.Isocode,
                                                         TotalBalance = Globalfunction.StringToDecimal(Encryption.DecryptID(_eventMark.TotalBalance.ToString(), balanceSalt)),
                                                         LastBalance = Globalfunction.StringToDecimal(Encryption.DecryptID(_eventMark.LastBalance.ToString(), balanceSalt)),
                                                         TargetBalance = _eventMark.TargetBalance != null ? Globalfunction.StringToDecimal(Encryption.DecryptID(_eventMark.TargetBalance.ToString(), balanceSalt)) : null,

                                                     }).ToList(),
                                       AddressResponse = (from _add in _db.EventAddresses
                                                          join _atype in _db.AddressTypes on _add.AddressId equals _atype.AddressId
                                                          where _add.EventPostId == _ev.PostId
                                                          select new EventAddressResponse
                                                          {
                                                              Address = _add.AddressName,
                                                              AddresstypeName = _atype.Address
                                                          }).ToList(),
                                       EventImageList = _db.EventFiles.Where(x => x.EventPostId == _ev.PostId)
                                                        .Select(x => new EventFileInfo
                                                        {
                                                            fileIdval = Encryption.EncryptID(x.UrlId.ToString(), LoginUserId.ToString()),
                                                            imgfilename = x.Url,
                                                            imgDescription = x.UrlDescription
                                                        }).ToList(),
                                        
                                   }).ToListAsync();
                 */

                if(status.ToLower() == "active")
                {
                    var posts = await (from _post in _db.Posts
                                       where _post.Inactive == false && _post.PostType.ToLower() == "eventpost"
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
                    List<DashboardEventPostResponse> netResponse = new List<DashboardEventPostResponse>();
                    foreach (var apost in posts)
                    {
                        var eventQuery = await (from _ev in _db.Events
                                                join _cre in _db.Users on _ev.CreatorId equals _cre.UserId
                                                join _sta in _db.StatusTypes on _ev.StatusId equals _sta.StatusId
                                                join _meship in _db.ChannelMemberships on _ev.ChannelId equals _meship.ChannelId
                                                join _usertype in _db.UserTypes on _meship.UserTypeId equals _usertype.TypeId
                                                where apost.PostId == _ev.PostId &&
                                                      _sta.StatusName.ToLower() == "approved" &&
                                                      _ev.ChannelId == ChannelId &&
                                                      (_ev.StartDate <= dateTime && _ev.EndDate >= dateTime) &&
                                                       (apost.ViewPolicies.GroupMemberOnly != null 
                                                       && apost.ViewPolicies.GroupMemberOnly == true 
                                                       ? _meship.UserId == LoginUserId : true)
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
                                             group new { _eventMark, _mark, _evall } by _eventMark.MarkId into groupedMarks
                                             select new
                                             {
                                                 EventPostId = groupedMarks.First()._eventMark.EventPostId,
                                                 Mark = new EventMarks
                                                 {
                                                     MarkIdval = Encryption.EncryptID(groupedMarks.Key.ToString(), LoginUserId.ToString()),
                                                     IsoCode = groupedMarks.First()._mark.Isocode,
                                                     MarkName = groupedMarks.First()._mark.MarkName,
                                                     AllowedMarkName = groupedMarks.Select(g => g._evall.AllowedMarkName).Distinct().FirstOrDefault(),
                                                     TotalBalance = Globalfunction.StringToDecimal(Encryption.DecryptID(groupedMarks.First()._eventMark.TotalBalance.ToString(), balanceSalt)),
                                                     LastBalance = Globalfunction.StringToDecimal(Encryption.DecryptID(groupedMarks.First()._eventMark.LastBalance.ToString(), balanceSalt)),
                                                     TargetBalance = groupedMarks.First()._eventMark.TargetBalance != null
                            ? Globalfunction.StringToDecimal(Encryption.DecryptID(groupedMarks.First()._eventMark.TargetBalance!.ToString(), balanceSalt))
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

                    Pagination pagination = RepoFunService.getWithPagination(payload.PageNumber, payload.PageSize, netResponse);
                    result = Result<Pagination>.Success(pagination);
                }
                else
                {
                    var eventQuery = await (from _ev in _db.Events
                                            join _post in _db.Posts on _ev.PostId equals _post.PostId
                                            join _cre in _db.Users on _ev.CreatorId equals _cre.UserId
                                            join _sta in _db.StatusTypes on _ev.StatusId equals _sta.StatusId
                                            join _meship in _db.ChannelMemberships on _ev.ChannelId equals _meship.ChannelId
                                            join _usertype in _db.UserTypes on _meship.UserTypeId equals _usertype.TypeId
                                            where _ev.ChannelId == ChannelId &&
                                                  _post.PostType == "eventpost" &&
                                                  _sta.StatusName.ToLower() == "approved" &&
                                                  _post.Inactive == false &&
                                                  _meship.UserId == LoginUserId && (
                                                  status.ToLower() == "active" ?
                                                      _ev.StartDate <= dateTime && _ev.EndDate >= dateTime :
                                                  status == "last" ?
                                                      _ev.EndDate.Year == dateTime.Year &&
                                                      _ev.EndDate.Month == dateTime.Month &&
                                                      _ev.EndDate.Day == dateTime.Day :
                                                  status == "upcoming" ?
                                                      _ev.StartDate.Year == dateTime.Year &&
                                                      _ev.StartDate.Month == dateTime.Month &&
                                                      _ev.StartDate.Day == dateTime.Day : false)
                                            select new
                                            {
                                                Event = _ev,
                                                Post = _post,
                                                Creator = _cre,
                                                EventMarks = _ev.PostId,
                                                EventAddresses = _ev.PostId,
                                                EventFiles = _ev.PostId
                                            }).ToListAsync();
                    var eventIds = eventQuery.Select(e => e.Event.PostId).ToList();

                    // Fetch EventMarks
                    var eventMarks = (from _eventMark in _db.EventMarkBalances
                                      join _mark in _db.Marks on _eventMark.MarkId equals _mark.MarkId
                                      join _evall in _db.EventAllowedMarks on _eventMark.MarkId equals _evall.MarkId
                                      where eventIds.Contains(_eventMark.EventPostId)
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
                                          where eventIds.Contains(_add.EventPostId)
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
                                        .Where(x => eventIds.Contains(x.EventPostId))
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
                    var finalResult = eventQuery.Select(e => new GetRequestEventResponse
                    {
                        EventPostIdval = Encryption.EncryptID(e.Event.PostId.ToString(), LoginUserId.ToString()),
                        EventName = e.Event.EventName,
                        EventDescrition = e.Event.EventDescription,
                        CreatorIdval = Encryption.EncryptID(e.Creator.UserId.ToString(), LoginUserId.ToString()),
                        CreatorName = e.Creator.Name,
                        StartDate = e.Event.StartDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                        EndDate = e.Event.EndDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                        ModifiedDate = e.Post.ModifiedDate.ToString("yyyy-MM-ddTHH:mm:ss"),

                        // EventMarks
                        EventMarks = eventMarks.Where(m => m.EventPostId == e.Event.PostId).Select(m => m.Mark).ToList(),

                        // AddressResponse
                        AddressResponse = eventAddresses.Where(a => a.EventPostId == e.Event.PostId).Select(a => a.AddressResponse).ToList(),

                        // EventImageList
                        EventImageList = eventFiles.Where(f => f.EventPostId == e.Event.PostId).Select(f => f.FileInfo).ToList()

                    }).ToList();

                    Pagination pagination = RepoFunService.getWithPagination(payload.PageNumber, payload.PageSize, finalResult);
                    result = Result<Pagination>.Success(pagination);
                }
            }
            else
            {
                result = Result<Pagination>.Error("Input Formot is Wrong");
            }
        }
        catch (Exception ex)
        {
            result = Result<Pagination>.Error(ex);
        }
        return result;
    }

    //TotalBalance = Globalfunction.StringToDecimal(Encryption.DecryptID(_ev.TotalBalance.ToString(), balanceSalt)),
    //LastBalance = Globalfunction.StringToDecimal(Encryption.DecryptID(_ev.LastBalance.ToString(), balanceSalt)),
    //TargetBalance = !string.IsNullOrEmpty(_ev.TargetBalance) ? Globalfunction.StringToDecimal(Encryption.DecryptID(_ev.TargetBalance!.ToString(), balanceSalt)) : 0,

    public async Task<Result<string>> CreateAllowedMarks(CreateAllowedMarkPayload payload, int LoginUserId)
    {
        Result<string> result = null;
        try
        {
            string balanceSalt = _configuration["appSettings:BalanceSalt"] ?? throw new Exception("Invalid Balance Salt");
            if (!string.IsNullOrEmpty(payload.EventIdval))
            {
                int EventPostId = Convert.ToInt32(Encryption.DecryptID(payload.EventIdval, LoginUserId.ToString()));
                var eventp = await _db.Events.Where(x => x.PostId == EventPostId).FirstOrDefaultAsync();
                if(eventp is not null)
                {
                    foreach (var item in payload.AllowMarkPayloads)
                    {
                        int MarkId= Convert.ToInt32(Encryption.DecryptID(item.MarkIdval, LoginUserId.ToString()));
                        var mark = await _db.Marks.Where(x => x.MarkId == MarkId).FirstOrDefaultAsync();
                        if(mark is not null)
                        {
                            ///check allow mark have been created
                            var allowMark = await _db.EventAllowedMarks.Where(x=> x.MarkId==MarkId && x.EventPostId==EventPostId).FirstOrDefaultAsync();
                            if(allowMark is null)
                            {
                                EventAllowedMark newMark = new EventAllowedMark
                                {
                                    AllowedMarkName = item.MarkName,
                                    MarkId = MarkId,
                                    EventPostId = EventPostId
                                };
                                await _db.EventAllowedMarks.AddAsync(newMark);
                                await _db.SaveChangesAsync();


                                await CreateNewEventAndChannelMarkBalance(MarkId, EventPostId,item.TargetBalance, balanceSalt);
                            }


                            ///Create Exchange Rate
                            foreach (var newer in item.ExchangeRatePayloads)
                            {
                                if (!string.IsNullOrEmpty(newer.ToMarkIdval))
                                {
                                    int ToMarkId = Convert.ToInt32(
                                        Encryption.DecryptID(newer.ToMarkIdval, LoginUserId.ToString()));
                                    var oldex = await _db.ExchangeRates.Where(x => x.FromMarkId == MarkId
                                    && x.ToMarkId == ToMarkId
                                    && x.EventPostId == EventPostId
                                    && x.MinQuantity == newer.MinQuantity).FirstOrDefaultAsync();
                                    if (oldex is null)
                                    {
                                        ExchangeRate newexra = new ExchangeRate
                                        {
                                            FromMarkId = MarkId,
                                            ToMarkId = ToMarkId,
                                            EventPostId = EventPostId,
                                            MinQuantity = newer.MinQuantity,
                                            Rate = newer.Rate
                                        };
                                        await _db.ExchangeRates.AddAsync(newexra);
                                        await _db.SaveChangesAsync();

                                        decimal? ExchTarget = item.TargetBalance * newer.Rate;
                                        await CreateNewEventAndChannelMarkBalance(ToMarkId, EventPostId, ExchTarget, balanceSalt);
                                    }
                                };
                            }
                        }
                    }

                    result = Result<string>.Success("Create Success");
                }
                else
                {
                    result = Result<string>.Error("Event Not Found");
                }
            }
            else
            {
                result = Result<string>.Error("Event Can't be Null");
            }
        }
        catch (Exception ex)
        {
            result = Result<string>.Error(ex);
        }
        return result;
    }

    public async Task<Result<string>> UpdateAllowdedMark(UpdateAllowdMarkPayload payload, int LoginUserId)
    {
        Result<string> result = null;
        try
        {
            /////Note: Updating allow event mark balance total is 0 and last mark balance is 0
            ///
            string balanceSalt = _configuration["appSettings:BalanceSalt"] ?? throw new Exception("Invalid Balance Salt");
            if (string.IsNullOrEmpty(payload.AllowdedMarkIdval))
                return Result<string>.Error("Invalide AllowedMark Id");

            int AllowedMarkId = Convert.ToInt32(Encryption.DecryptID(payload.AllowdedMarkIdval, LoginUserId.ToString()));
            var allowedMark = await _db.EventAllowedMarks.Where(x => x.AllowedMarkId == AllowedMarkId).FirstOrDefaultAsync();
            if (allowedMark is null)
                return Result<string>.Error("Allowed Mark Not Found");

            int FromMarkId = Convert.ToInt32(Encryption.DecryptID(payload.FromMarkIdval, LoginUserId.ToString()));
            var mark = await _db.Marks.Where(x => x.MarkId == FromMarkId).FirstOrDefaultAsync();
            if (mark is null)
                return Result<string>.Error("FromMark Not Found");
            int EventPostId = allowedMark.EventPostId;

            var eventBalance = await _db.EventMarkBalances.Where(x => x.EventPostId == EventPostId
            && x.MarkId == allowedMark.MarkId).FirstOrDefaultAsync();
            if (eventBalance is not null)
            {
                decimal TotalBalance = Globalfunction.StringToDecimal(Encryption.DecryptID(eventBalance.TotalBalance, balanceSalt));
                decimal LastBalance = Globalfunction.StringToDecimal(Encryption.DecryptID(eventBalance.LastBalance, balanceSalt));
                if ((TotalBalance == 0) && (LastBalance == 0))
                {
                    string? TargetBalance = null;
                    if (payload.TargetBalance is not null)
                    {
                        TargetBalance = Encryption.EncryptID(payload.TargetBalance.Value.ToString(), balanceSalt);
                    }
                    eventBalance.TargetBalance = TargetBalance;
                    await _db.SaveChangesAsync();

                    allowedMark.MarkId = FromMarkId;
                    allowedMark.AllowedMarkName = payload.MarkName;
                    await _db.SaveChangesAsync();
                    ///Check Channel Mark Balance
                    int ChannelId = await _db.Events.Where(x => x.PostId == EventPostId).Select(x => x.ChannelId).FirstOrDefaultAsync();
                    if (ChannelId > 0)
                    {
                        var chanBalance = await _db.ChannelMarkBalances.Where(x => x.ChannelId == ChannelId && x.MarkId == FromMarkId).FirstOrDefaultAsync();
                        if (chanBalance is null)
                        {
                            string ChannelTotalBalance = Encryption.EncryptID("0.0", balanceSalt);
                            string ChannelLastBalance = Encryption.EncryptID("0.0", balanceSalt);
                            ChannelMarkBalance newBalance = new ChannelMarkBalance
                            {
                                ChannelId = ChannelId,
                                MarkId = FromMarkId,
                                TotalBalance = ChannelTotalBalance,
                                LastBalance = ChannelLastBalance,
                            };
                            await _db.ChannelMarkBalances.AddAsync(newBalance);
                            await _db.SaveChangesAsync();
                        }
                    }
                    List<UpdateExchangeRatePayload> expayload = payload.UpdateExchangeRatePayloads;
                    foreach (var item in expayload)
                    {
                        int ToMarkId = Convert.ToInt32(Encryption.DecryptID(item.ToMarkIdval, LoginUserId.ToString()));
                        if (string.IsNullOrEmpty(item.ExchangeRateIdval))
                        {
                            int ExchangeRateId = Convert.ToInt32(Encryption.DecryptID(item.ExchangeRateIdval!, LoginUserId.ToString()));

                            var exrate = await _db.ExchangeRates.Where(x => x.ExchangeRateId == ExchangeRateId).FirstOrDefaultAsync();
                            if (exrate is not null)
                            {
                                exrate.FromMarkId = FromMarkId;
                                exrate.ToMarkId = ToMarkId;
                                exrate.MinQuantity = item.MinQuantity;
                                exrate.Rate = item.Rate;

                                await _db.SaveChangesAsync();
                            }
                        }
                        else
                        {
                            ExchangeRate newrate = new ExchangeRate
                            {
                                FromMarkId = FromMarkId,
                                ToMarkId = ToMarkId,
                                EventPostId = EventPostId,
                                MinQuantity = item.MinQuantity,
                                Rate = item.Rate,
                            };
                            await _db.ExchangeRates.AddAsync(newrate);
                            await _db.SaveChangesAsync();
                        }
                        if (ChannelId > 0)
                        {
                            var chanBalance = await _db.ChannelMarkBalances.Where(x => x.ChannelId == ChannelId && x.MarkId == ToMarkId).FirstOrDefaultAsync();
                            if (chanBalance is null)
                            {
                                string ChannelTotalBalance = Encryption.EncryptID("0.0", balanceSalt);
                                string ChannelLastBalance = Encryption.EncryptID("0.0", balanceSalt);
                                ChannelMarkBalance newBalance = new ChannelMarkBalance
                                {
                                    ChannelId = ChannelId,
                                    MarkId = FromMarkId,
                                    TotalBalance = ChannelTotalBalance,
                                    LastBalance = ChannelLastBalance,
                                };
                                await _db.ChannelMarkBalances.AddAsync(newBalance);
                                await _db.SaveChangesAsync();
                            }
                        }
                    }
                }

            }
            ///Update Mark Balance Target

        }
        catch (Exception ex)
        {
            result = Result<string>.Error(ex);
        }
        return result;
    }

    private async Task CreateNewEventAndChannelMarkBalance(int MarkId,int EventPostId, decimal? Targetbalance,string balanceSalt)
    {
        ///next check event mark balance
        var evbalance = await _db.EventMarkBalances.Where(x => x.MarkId == MarkId && x.EventPostId == EventPostId).FirstOrDefaultAsync();
        if (evbalance is null)
        {
            string TotalBalance = Encryption.EncryptID("0.0", balanceSalt);
            string LastBalance = Encryption.EncryptID("0.0", balanceSalt);
            string? TargetBalance = Targetbalance != null ? Encryption.EncryptID(Targetbalance.Value.ToString(), balanceSalt) : null;
            EventMarkBalance newBalance = new EventMarkBalance
            {
                EventPostId = EventPostId,
                MarkId = MarkId,
                TotalBalance = TotalBalance,
                LastBalance = LastBalance,
                TargetBalance = TargetBalance
            };
            await _db.EventMarkBalances.AddAsync(newBalance);
            await _db.SaveChangesAsync();
        }
        else
        {
            if(Targetbalance is not null)
            {

                string? TagBal = evbalance.TargetBalance;
                if (TagBal is not null)
                {
                    decimal Target = Globalfunction.StringToDecimal(Encryption.DecryptID(TagBal, balanceSalt));
                    decimal NewTarget = Target + Targetbalance.Value;
                    string NewTargetString = Encryption.EncryptID(NewTarget.ToString(), balanceSalt);
                    evbalance.TargetBalance = NewTargetString;
                    await _db.SaveChangesAsync();
                }
                else
                {
                    string NewTargetString = Encryption.EncryptID(Targetbalance.Value.ToString(), balanceSalt);
                    evbalance.TargetBalance = NewTargetString;
                    await _db.SaveChangesAsync();
                }
            }
        }

        ///also check channel mark balance
        int ChannelId = await _db.Events.Where(x => x.PostId == EventPostId).Select(x => x.ChannelId).FirstOrDefaultAsync();
        if (ChannelId > 0)
        {
            var chanBalance = await _db.ChannelMarkBalances.Where(x => x.ChannelId == ChannelId && x.MarkId == MarkId).FirstOrDefaultAsync();
            if (chanBalance is null)
            {
                string TotalBalance = Encryption.EncryptID("0.0", balanceSalt);
                string LastBalance = Encryption.EncryptID("0.0", balanceSalt);
                ChannelMarkBalance newBalance = new ChannelMarkBalance
                {
                    ChannelId = ChannelId,
                    MarkId = MarkId,
                    TotalBalance = TotalBalance,
                    LastBalance = LastBalance,
                };
                await _db.ChannelMarkBalances.AddAsync(newBalance);
                await _db.SaveChangesAsync();
            }
        }
    }

    public async Task<Result<Pagination>> GetAllowedMarks(GetAllowedMarkPayload payload, int LoginUserId)
    {
        Result<Pagination> result = null;
        try
        {
            int EventPostId = Convert.ToInt32(Encryption.DecryptID(payload.EventIdval!, LoginUserId.ToString()));
            if (string.IsNullOrEmpty(payload.MarkTypeIdval) || payload.MarkTypeIdval.ToLower() == "all")
            {
                var query = await (from _evmk in _db.EventAllowedMarks
                                   join _mark in _db.Marks on _evmk.MarkId equals _mark.MarkId
                                   where _evmk.EventPostId == EventPostId
                                   select new
                                   {
                                       AllowedMarkId = _evmk.AllowedMarkId,
                                       MarkId = _mark.MarkId,
                                       IsoCode = _mark.Isocode,
                                       AllowedMarkName = _evmk.AllowedMarkName,
                                   }).ToListAsync();

                List<AllowedMarkResponse> allowedMarks = new List<AllowedMarkResponse>();

                foreach (var item in query)
                {
                    // Get exchange rates for the current mark
                    /*var newexchange = await (from _ex in _db.ExchangeRates
                                             join _mks in _db.Marks on _ex.ToMarkId equals _mks.MarkId
                                             where _ex.FromMarkId == item.MarkId
                                             && _ex.EventPostId == EventPostId
                                             orderby _ex.MinQuantity descending
                                             select new ExchangeRateResponse
                                             {
                                                 ToMarkIdval = Encryption.EncryptID(_mks.MarkId.ToString(), LoginUserId.ToString()),
                                                 MarkName = _mks.MarkName,
                                                 IsoCode = _mks.Isocode,
                                                 MinQuantiry = _ex.MinQuantity,
                                                 Rate = _ex.Rate
                                             }).ToListAsync();
                     */
                    var newexchange = await (from _ex in _db.ExchangeRates
                                             join _mks in _db.Marks on _ex.ToMarkId equals _mks.MarkId
                                             where _ex.FromMarkId == item.MarkId
                                             && _ex.EventPostId == EventPostId
                                             group _ex by new
                                             {
                                                 _ex.ExchangeRateId,
                                                 _mks.MarkId,
                                                 _mks.MarkName,
                                                 _mks.Isocode
                                             } into g
                                             select new ExchangeRateResponse
                                             {
                                                 ExchangeRateIdval = Encryption.EncryptID(g.Key.ExchangeRateId.ToString(),LoginUserId.ToString()),
                                                 ToMarkIdval = Encryption.EncryptID(g.Key.MarkId.ToString(), LoginUserId.ToString()),
                                                 MarkName = g.Key.MarkName,
                                                 IsoCode = g.Key.Isocode,
                                                 Rates = g.Select(x => new RatesResponse
                                                 {
                                                     MinQuantity = x.MinQuantity,
                                                     Rate = x.Rate
                                                 }).OrderByDescending(x => x.MinQuantity).ToList()
                                             }).ToListAsync();


                    // Create new AllowedMarkResponse and attach exchange rates
                    AllowedMarkResponse newallow = new AllowedMarkResponse
                    {
                        AllowedMarkIdval = Encryption.EncryptID(item.AllowedMarkId.ToString(), LoginUserId.ToString()),
                        MarkIdval = Encryption.EncryptID(item.MarkId.ToString(), LoginUserId.ToString()),
                        AllowedMarkName = item.AllowedMarkName,
                        IsoCode = item.IsoCode,
                        ExchangeRates = newexchange // Add exchange rates here
                    };

                    allowedMarks.Add(newallow);
                }


                //AllowedMarkResponse
                Pagination pagination = RepoFunService.getWithPagination(payload.PageNumber, payload.PageSize, allowedMarks);
                result = Result<Pagination>.Success(pagination);
            }
            else
            {
                int MarkTypeId = Convert.ToInt32(Encryption.DecryptID(payload.MarkTypeIdval!, LoginUserId.ToString()));
                var query = await (from _evmk in _db.EventAllowedMarks
                                         join _mark in _db.Marks on _evmk.MarkId equals _mark.MarkId
                                         join _mt in _db.MarkTypes on _mark.MarkTypeId equals _mt.MarkTypeId
                                         where _evmk.EventPostId == EventPostId && _mt.MarkTypeId == MarkTypeId
                                         select new
                                         {
                                             AllowedMarkId = _evmk.AllowedMarkId,
                                             MarkId = _mark.MarkId,
                                             IsoCode = _mark.Isocode,
                                             AllowedMarkName = _evmk.AllowedMarkName,
                                         }).ToListAsync();
                List<AllowedMarkResponse> allowedMarks = new List<AllowedMarkResponse>();

                foreach (var item in query)
                {
                    var newexchange = await (from _ex in _db.ExchangeRates
                                             join _mks in _db.Marks on _ex.ToMarkId equals _mks.MarkId
                                             where _ex.FromMarkId == item.MarkId
                                             && _ex.EventPostId == EventPostId
                                             group _ex by new
                                             {
                                                 _mks.MarkId,
                                                 _mks.MarkName,
                                                 _mks.Isocode
                                             } into g
                                             select new ExchangeRateResponse
                                             {
                                                 ToMarkIdval = Encryption.EncryptID(g.Key.MarkId.ToString(), LoginUserId.ToString()),
                                                 MarkName = g.Key.MarkName,
                                                 IsoCode = g.Key.Isocode,
                                                 Rates = g.Select(x => new RatesResponse
                                                 {
                                                     MinQuantity = x.MinQuantity,
                                                     Rate = x.Rate
                                                 }).OrderByDescending(x => x.MinQuantity).ToList()
                                             }).ToListAsync();


                    // Create new AllowedMarkResponse and attach exchange rates
                    AllowedMarkResponse newallow = new AllowedMarkResponse
                    {
                        AllowedMarkIdval = Encryption.EncryptID(item.AllowedMarkId.ToString(), LoginUserId.ToString()),
                        MarkIdval = Encryption.EncryptID(item.MarkId.ToString(), LoginUserId.ToString()),
                        AllowedMarkName = item.AllowedMarkName,
                        IsoCode = item.IsoCode,
                        ExchangeRates = newexchange // Add exchange rates here
                    };

                    allowedMarks.Add(newallow);
                }
                Pagination pagination = RepoFunService.getWithPagination(payload.PageNumber, payload.PageSize, allowedMarks);
                result = Result<Pagination>.Success(pagination);
            }
        }
        catch (Exception ex)
        {
            result = Result<Pagination>.Error(ex);
        }
        return result;
    }


    public async Task<Result<Pagination>> GetEventSupervisors(GetEventData payload, int LoginUserId)
    {
        Result<Pagination> result = null;
        try
        {
            int EventPostId = Convert.ToInt32(Encryption.DecryptID(payload.EventIdval, LoginUserId.ToString()));
            var query = await (from ep in _db.Posts
                               join _ev in _db.Events on ep.PostId equals _ev.PostId
                               join _em in _db.EventMemberships on ep.PostId equals _em.EventPostId
                               join _chh in _db.Channels on _ev.ChannelId equals _chh.ChannelId
                               join _cm in _db.ChannelMemberships on _chh.ChannelId equals _cm.ChannelId
                               join _ut in _db.UserTypes on _em.UserTypeId equals _ut.TypeId
                               join _user in _db.Users on _em.UserId equals _user.UserId
                               where ep.PostId == EventPostId
                               && (_ut.Name.ToLower().Contains("owner")
                               || _ut.Name.ToLower().Contains("admin"))
                               && _em.UserId == _cm.UserId
                               select new
                               {
                                   UserIdval = Encryption.EncryptID(_user.UserId.ToString(), LoginUserId.ToString()),
                                   UserName = _user.Name,
                                   Content = _user.Email ?? _user.Phone,
                                   UserType = _ut.Name,
                                   JoinedDate = _cm.JoinedDate,
                                   ProfileImage = _db.UserProfiles.Where(x => x.UserId == _user.UserId)
                                                        .OrderByDescending(x => x.CreatedDate)
                                                        .Select(x => x.Url).LastOrDefault()
                               }).ToListAsync();
            Pagination data = RepoFunService.getWithPagination(payload.pageNumber, payload.pageSize, query);
            result = Result<Pagination>.Success(data);
        }
        catch (Exception ex)
        {
            result = Result<Pagination>.Error(ex);
        }
        return result;
    }


    public async Task<Result<EventMenuAccess>> CheckEventAccessMenu(GetEventDataPayload payload, int LoginUserId)
    {
        Result<EventMenuAccess> result = null;
        try
        {
            EventMenuAccess menuAccess = new EventMenuAccess
            {
                CanPostReview = false,
                CanPostAction = false,
            };
            int EventPostId = Convert.ToInt32(Encryption.DecryptID(payload.EventPostIdval!, LoginUserId.ToString()));
            var admin = await (from _ev in _db.Events
                               join _em in _db.EventMemberships on _ev.PostId equals _em.EventPostId
                               join _ut in _db.UserTypes on _em.UserTypeId equals _ut.TypeId
                               where _ev.PostId == EventPostId &&
                               _em.UserId == LoginUserId &&
                               (_ut.Name.ToLower() == "admin" || _ut.Name.ToLower() == "owner")
                               select _em
                               ).FirstOrDefaultAsync();
            if(admin is not null)
            {
                menuAccess.CanPostReview = true;
                menuAccess.CanPostAction = true;
            }
            result = Result<EventMenuAccess>.Success(menuAccess);
        }
        catch (Exception ex)
        {
            result = Result<EventMenuAccess>.Error(ex);
        }
        return result;
    }


    public async Task<Result<Pagination>> FindAccessEventByName(FindByNamePayload payload, int LoginUserId)
    {
        Result<Pagination> result = null;
        try
        {
            DateTime now = DateTime.Now;
            string? Name = payload.Name;
            List<EventNameData> query = await (from _po in _db.Posts
                                              join _ev in _db.Events on _po.PostId equals _ev.PostId
                                              join _ch in _db.Channels on _ev.ChannelId equals _ch.ChannelId
                                              join _cm in _db.ChannelMemberships on _ch.ChannelId equals _cm.ChannelId
                                              join _status in _db.StatusTypes on _cm.StatusId equals _status.StatusId
                                              where _cm.UserId == LoginUserId && _status.StatusName.ToLower() == "approved" &&
                                              _ev.StartDate <= now && now <= _ev.EndDate 
                                              && (string.IsNullOrEmpty(Name) ? true : _ev.EventName.Contains(Name))
                                              select new EventNameData
                                              {
                                                  EventIdval = Encryption.EncryptID(_po.PostId.ToString(), LoginUserId.ToString()),
                                                  EventName = _ev.EventName
                                              }).ToListAsync();
            Pagination data = RepoFunService.getWithPagination(payload.PageNumber, payload.PageSize, query);
            result = Result<Pagination>.Success(data);
        }
        catch (Exception ex)
        {
            result = Result<Pagination>.Error(ex);
        }
        return result;
    }

    public async Task<Result<Pagination>> EventContributionFilterMarkId(EventContributionPayload payload, int LoginUserId)
    {
        Result<Pagination> result = null;
        try
        {
            string balanceSalt = _configuration["appSettings:BalanceSalt"] ?? throw new Exception("Invalid Balance Salt");
            int EventPostId = Convert.ToInt32(Encryption.DecryptID(payload.EventIdval, LoginUserId.ToString()));
            int MarkId = Convert.ToInt32(Encryption.DecryptID(payload.MarkIdval, LoginUserId.ToString()));
            ///need to output (name,userIdval photo,userlevel,contriubte percentage)

            /*
             var query = await (from _ev in _db.Events
                              join _coll in _db.CollectPosts on _ev.PostId equals _coll.EventPostId
                              join _user in _db.Users on _coll.CreatorId equals _user.UserId
                              where _ev.PostId == EventPostId && _coll.MarkId == MarkId
                              group new { _coll, _user } by new { _user.UserId, _user.Name } into userGroup
                              orderby userGroup.Sum(x => Globalfunction.StringToDecimal(Encryption.DecryptID(x._coll.CollectAmount, balanceSalt))) descending
                              select new
                              {
                                  UserId = userGroup.Key.UserId,
                                  UserName = userGroup.Key.Name,
                                  TotalCollectedAmount = userGroup.Sum(x => Globalfunction.StringToDecimal(Encryption.DecryptID(x._coll.CollectAmount, balanceSalt)))
                              }
                              ).ToListAsync();
                                //join _cm in _db.EventMemberships on _user.UserId equals _cm.UserId into _cmg
                                //from _cmJoined in _cmg.DefaultIfEmpty()
                                //join _ut in _db.UserTypes on _cmJoined.UserTypeId equals _ut.TypeId into _utg
                                //from _utJoined in _utg.DefaultIfEmpty()
                                   //UserTypeId = _utJoined != null ? _utJoined.TypeId : 0,
                                   UserTypeName = _utJoined != null ? _utJoined.Name : null,
            */

            /*
             var query = await (from _ev in _db.Events
                               join _coll in _db.CollectPosts on _ev.PostId equals _coll.EventPostId
                               join _colBal in _db.PostBalances on _coll.PostId equals _colBal.PostId
                               join _eb in _db.EventMarkBalances on _ev.PostId equals _eb.EventPostId
                               join _user in _db.Users on _coll.CreatorId equals _user.UserId
                                where _ev.PostId == EventPostId && _colBal.MarkId == MarkId
                               select new
                               {
                                   UserId = _user.UserId,
                                   UserName = _user.Name,
                                   Contact = _user.Email,
                                   CollectBalance = Globalfunction.StringToDecimal(Encryption.DecryptID(_coll.CollectAmount.ToString(), balanceSalt)),
                                   TotalBalance = Globalfunction.StringToDecimal(Encryption.DecryptID(_eb.TotalBalance.ToString(), balanceSalt))
                               }
                               ).ToListAsync();*/
            var query = await (from _ev in _db.Events
                               join _coll in _db.CollectPosts on _ev.PostId equals _coll.EventPostId
                               join _colBal in _db.PostBalances on _coll.PostId equals _colBal.PostId
                               join _eb in _db.EventMarkBalances on _ev.PostId equals _eb.EventPostId
                               join _user in _db.Users on _coll.CreatorId equals _user.UserId
                               where _ev.PostId == EventPostId && _colBal.MarkId == MarkId && _eb.MarkId == MarkId
                               select new
                               {
                                   UserId = _coll.CreatorId,
                                   UserName = _user.Name,
                                   Contact = _user.Email,
                                   CollectBalance = Globalfunction.StringToDecimal(Encryption.DecryptID(_colBal.Balance, balanceSalt)),
                                   TotalBalance = Globalfunction.StringToDecimal(Encryption.DecryptID(_eb.TotalBalance, balanceSalt))
                               }
                   ).ToListAsync();


            var groupedResult = query
                                .GroupBy(x => new { x.UserId, x.UserName, x.Contact,x.TotalBalance })
                                .Select(g => new
                                {
                                    UserIdval = Encryption.EncryptID(g.Key.UserId.ToString(),LoginUserId.ToString()),
                                    UserName = g.Key.UserName,
                                    Contact = g.Key.Contact,
                                    TotalCollectBalance = g.Sum(x => x.CollectBalance),
                                    TotalBalance = g.Key.TotalBalance
                                })
                                .OrderByDescending(g => g.TotalCollectBalance)
                                .ToList();
            Pagination pa = RepoFunService.getWithPagination(payload.pageNumber, payload.pageSize, groupedResult);
            result = Result<Pagination>.Success(pa);
        }
        catch (Exception ex)
        {
            result = Result<Pagination>.Error(ex);
        }
        return result;
    }
    public async Task<Result<Pagination>> GetUserContributons(GetUserContributonsPayload payload, int LoginUserId)
    {
        Result<Pagination> result = null;
        try
        {
            string balanceSalt = _configuration["appSettings:BalanceSalt"] ?? throw new Exception("Invalid Balance Salt");
            int? UserId = null;
            if (!string.IsNullOrEmpty(payload.UserIdval))
            {
                UserId = Convert.ToInt32(Encryption.DecryptID(payload.UserIdval, LoginUserId.ToString()));
            }

            

            var evquery = (from _ev in _db.Events
                                 join _status in _db.StatusTypes on _ev.StatusId equals _status.StatusId
                                 join _me in _db.EventMemberships on _ev.PostId equals _me.EventPostId
                                 join _ch in _db.Channels on _ev.ChannelId equals _ch.ChannelId
                                 join _chme in _db.ChannelMemberships on _ch.ChannelId equals _chme.ChannelId
                                 where (UserId != null ? _chme.UserId == UserId : _chme.UserId == LoginUserId)
                                 && _status.StatusName.ToLower() == "approved"
                           select _ev).ToList();
            List<dynamic> lastQuery = new List<dynamic>();
            foreach (var newevent in evquery)
            {
                var query = await (from _coll in _db.CollectPosts
                                   join _st in _db.StatusTypes on _coll.StatusId equals _st.StatusId
                                   join _colBal in _db.PostBalances on _coll.PostId equals _colBal.PostId
                                   join _eb in _db.EventMarkBalances on _coll.EventPostId equals _eb.EventPostId
                                   join _mk in _db.Marks on _eb.MarkId equals _mk.MarkId
                                   join _user in _db.Users on _coll.CreatorId equals _user.UserId
                                   where _colBal.MarkId == _eb.MarkId && _st.StatusName.ToLower() == "approved"
                                         && _coll.EventPostId == newevent.PostId
                                         && (UserId != null ? _coll.CreatorId == UserId : _coll.CreatorId == LoginUserId)
                                   group new
                                   {
                                       _mk.MarkId,
                                       _mk.MarkName,
                                       _mk.Isocode,
                                       _colBal.Balance, 
                                       _eb.TotalBalance 
                                   }
                                   by new
                                   {
                                       _mk.MarkId,
                                       _mk.MarkName,
                                       _mk.Isocode,
                                       _eb.TotalBalance 
                                   } into grouped
                                   select new
                                   {
                                       MarkIdval = grouped.Key.MarkId.ToString(), 
                                       MarkName = grouped.Key.MarkName,
                                       IsoCode = grouped.Key.Isocode,
                                       CollectBalance = grouped.Select(x => x.Balance).ToList(), 
                                       TotalBalance = grouped.Key.TotalBalance 
                                   }).ToListAsync();
                var queryresult = query.Select(x => new
                {
                    MarkIdval = Encryption.EncryptID(x.MarkIdval, LoginUserId.ToString()),
                    MarkName = x.MarkName,
                    IsoCode = x.IsoCode,
                    CollectBalance = x.CollectBalance
                        .Sum(bal => Globalfunction.StringToDecimal(Encryption.DecryptID(bal, balanceSalt))),
                    TotalBalance = Globalfunction.StringToDecimal(Encryption.DecryptID(x.TotalBalance, balanceSalt))
                }).ToList();

                var data = new
                {
                    EventPostIdval = Encryption.EncryptID(newevent.PostId.ToString(), LoginUserId.ToString()),
                    EventName = newevent.EventName,
                    Balances = queryresult
                };
                lastQuery.Add(data);
            }
            Pagination pa = RepoFunService.getWithPagination(payload.pageNumber, payload.pageSize, lastQuery);
            result = Result<Pagination>.Success(pa);
        }
        catch (Exception ex)
        {
            result = Result<Pagination>.Error(ex);
        }
        return result;
    }

    public async Task<Result<Pagination>> EventOverallContributions(OverallContributionPayload payload, int LoginUserId)
    {
        Result<Pagination> result = null;
        try
        {
            string balanceSalt = _configuration["appSettings:BalanceSalt"] ?? throw new Exception("Invalid Balance Salt");
            int EventPostId = Convert.ToInt32(Encryption.DecryptID(payload.Idval, LoginUserId.ToString()));

            // Fetch allowed marks for the event
            List<Mark> allowedMarks = await (from _mark in _db.Marks
                                             join _eb in _db.EventMarkBalances on _mark.MarkId equals _eb.MarkId
                                             where _eb.EventPostId == EventPostId
                                             select _mark)
                                .Distinct()
                                .ToListAsync();
            List<OverallContributionsResponse> overallContributions = new List<OverallContributionsResponse>();

            if (!allowedMarks.Any())
            {
                Pagination pa1 = RepoFunService.getWithPagination(payload.pageNumber, payload.pageSize, overallContributions);
                return Result<Pagination>.Success(pa1);
            }

            var firstMark = allowedMarks.FirstOrDefault();
            int firstMarkId = firstMark!.MarkId;
            string SortMarkIdval = Encryption.EncryptID(firstMarkId.ToString(), LoginUserId.ToString());
            if (!payload.MarkIdval.IsNullOrEmpty())
            {
                int SortMarkId = Convert.ToInt32(Encryption.DecryptID(payload.MarkIdval!, LoginUserId.ToString()));
                firstMarkId = allowedMarks.Where(x => x.MarkId == SortMarkId).Select(x => x.MarkId).FirstOrDefault();
                SortMarkIdval = Encryption.EncryptID(firstMarkId.ToString(), LoginUserId.ToString());
            }

            // Fetch the list of event members
            var members = await (from _user in _db.Users
                                        join _members in _db.ChannelMemberships on _user.UserId equals _members.UserId
                                        join _event in _db.Events on _members.ChannelId equals _event.ChannelId
                                        join _pro in _db.UserProfiles on _user.UserId equals _pro.UserId into profile
                                        where _event.PostId == EventPostId
                                        select new
                                        {
                                            UserId = _user.UserId,
                                            Name = _user.Name,
                                            Email = _user.Email,
                                            Image = profile.OrderByDescending(p => p.CreatedDate)
                                            .Select(x => x.Url)
                                            .FirstOrDefault()
                                        }).ToListAsync();


            foreach (var member in members)
            {
                List<ContributionResponse> contributions = new List<ContributionResponse>();

                foreach (var mark in allowedMarks)
                {
                    var query = await (from _coll in _db.CollectPosts
                                       join _st in _db.StatusTypes on _coll.StatusId equals _st.StatusId
                                       join _colBal in _db.PostBalances on _coll.PostId equals _colBal.PostId
                                       join _eb in _db.EventMarkBalances on _coll.EventPostId equals _eb.EventPostId
                                       where _colBal.MarkId == _eb.MarkId &&
                                             mark.MarkId == _colBal.MarkId &&
                                             mark.MarkId == _eb.MarkId &&
                                             (_st.StatusName.ToLower() == "approved") &&//|| _st.StatusName.ToLower() == "pending"
                                             _coll.EventPostId == EventPostId &&
                                             _coll.CreatorId == member.UserId
                                       group new
                                       {
                                           _colBal.Balance,
                                           _eb.TotalBalance
                                       }
                                       by _eb.TotalBalance into grouped
                                       select new
                                       {
                                           CollectBalance = grouped.Select(x => x.Balance).ToList(),
                                           TotalBalance = grouped.Key
                                       }).ToListAsync();

                    // If no contributions are found, add a default contribution for the mark
                    if (!query.Any())
                    {
                        string? totalBalance = await _db.EventMarkBalances
                                                  .Where(x => x.EventPostId == EventPostId && x.MarkId == mark.MarkId)
                                                  .Select(x => x.TotalBalance)
                                                  .FirstOrDefaultAsync();

                        contributions.Add(new ContributionResponse
                        {
                            MarkIdval = Encryption.EncryptID(mark.MarkId.ToString(), LoginUserId.ToString()),
                            MarkName = mark.MarkName,
                            IsoCode = mark.Isocode,
                            CollectBalance = 0,
                            TotalBalance = Globalfunction.StringToDecimal(Encryption.DecryptID(totalBalance!, balanceSalt))
                        });
                    }
                    else
                    {
                        // Process contributions from the query
                        contributions.AddRange(query.Select(x => new ContributionResponse
                        {
                            MarkIdval = Encryption.EncryptID(mark.MarkId.ToString(), LoginUserId.ToString()),
                            MarkName = mark.MarkName,
                            IsoCode = mark.Isocode,
                            CollectBalance = x.CollectBalance
                                .Sum(bal => Globalfunction.StringToDecimal(Encryption.DecryptID(bal, balanceSalt))),
                            TotalBalance = Globalfunction.StringToDecimal(Encryption.DecryptID(x.TotalBalance, balanceSalt))
                        }));
                    }
                }

                // Create the overall contribution response
                overallContributions.Add(new OverallContributionsResponse
                {
                    ContributorIdval = Encryption.EncryptID(member.UserId.ToString(), LoginUserId.ToString()),
                    ContributorName = member.Name,
                    Contact = member.Email ?? "",
                    UserImageUrl = member.Image ?? "",
                    contributions = contributions
                });
            }

            // Sort the overall contributions by the first mark (e.g., USD)
            overallContributions = overallContributions
                .OrderByDescending(over => over.contributions
                    .FirstOrDefault(c => c.MarkIdval == SortMarkIdval)?.CollectBalance ?? 0)
                .ToList();

            Pagination pa = RepoFunService.getWithPagination(payload.pageNumber, payload.pageSize, overallContributions);
            result = Result<Pagination>.Success(pa);
        }
        catch (Exception ex)
        {
            result = Result<Pagination>.Error(ex);
        }
        return result;
    }
    public async Task<Result<Pagination>> GetAllEvent(OrderByMonthPayload payload, int LoginUserId)
    {
        Result<Pagination> result = null;
        try
        {
                if (payload.Status is null || payload is null)
                    return Result<Pagination>.Error("Please add Stauts");

                if (payload.Status.ToLower() != "active" && payload.Status.ToLower() != "last" && payload.Status.ToLower() != "upcoming")
                    return Result<Pagination>.Error("Invalide Status");

                if (payload.Month is not null && payload.PageNumber >= 1 && payload.PageSize >= 1)
                {
                    CultureInfo provider = CultureInfo.InvariantCulture;
                    DateTime dateTime = DateTime.ParseExact(payload.Month, "yyyy-MM-dd", provider);
                    //int ChannelId = Convert.ToInt32(Encryption.DecryptID(payload.Idval!, LoginUserId.ToString()));
                    string status = payload.Status;
                    string balanceSalt = _configuration["appSettings:BalanceSalt"] ?? throw new Exception("Invalid Balance Salt");
                    
                    var posts = await (from _post in _db.Posts
                                           where _post.Inactive == false && _post.PostType.ToLower() == "eventpost"
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
                        List<DashboardEventPostResponse> netResponse = new List<DashboardEventPostResponse>();
                        foreach (var apost in posts)
                        {
                            var eventQuery = await (from _ev in _db.Events
                                                    join _cre in _db.Users on _ev.CreatorId equals _cre.UserId
                                                    join _sta in _db.StatusTypes on _ev.StatusId equals _sta.StatusId
                                                    join _meship in _db.ChannelMemberships on _ev.ChannelId equals _meship.ChannelId
                                                    join _meSta in _db.StatusTypes on _meship.StatusId equals _meSta.StatusId
                                                    join _usertype in _db.UserTypes on _meship.UserTypeId equals _usertype.TypeId
                                                    where apost.PostId == _ev.PostId &&
                                                          _sta.StatusName.ToLower() == "approved" &&
                                                          _meSta.StatusName.ToLower() == "approved" &&
                                                          (
                                                      status.ToLower() == "active" ?
                                                          _ev.StartDate <= dateTime && _ev.EndDate >= dateTime :
                                                      status == "last" ?
                                                          _ev.EndDate.Year == dateTime.Year &&
                                                          _ev.EndDate.Month == dateTime.Month &&
                                                          _ev.EndDate.Day == dateTime.Day :
                                                      status == "upcoming" ?
                                                          _ev.StartDate.Year == dateTime.Year &&
                                                          _ev.StartDate.Month == dateTime.Month &&
                                                          _ev.StartDate.Day == dateTime.Day : false) &&
                                                           ((apost.ViewPolicies.GroupMemberOnly != null 
                                                           && apost.ViewPolicies.GroupMemberOnly == true) ? _meship.UserId == LoginUserId : true)
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
                                          group new { _eventMark, _mark, _evall } by _eventMark.MarkId into groupedMarks
                                          select new
                                          {
                                              EventPostId = groupedMarks.First()._eventMark.EventPostId,
                                              Mark = new EventMarks
                                              {
                                                  MarkIdval = Encryption.EncryptID(groupedMarks.Key.ToString(), LoginUserId.ToString()),
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
                                    ModifiedDate = apost.ModifiedDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                                    CreatedDate = apost.CreatedDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                                    ViewTotalCount = apost.Views,
                                    LikeTotalCount = apost.Likes,
                                    CommandTotalCount = apost.Commands,
                                    ShareTotalCount = apost.Shares,
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

                        Pagination pagination = RepoFunService.getWithPagination(payload.PageNumber, payload.PageSize, netResponse);
                        result = Result<Pagination>.Success(pagination);
                }
                else
                {
                    result = Result<Pagination>.Error("Input Formot is Wrong");
                }
            }
        catch (Exception ex)
        {
            result = Result<Pagination>.Error(ex);
        }
        return result;
    }
}
