using Amazon;
using Humanizer;
using KoiCoi.Models.EventDto.Payload;
using KoiCoi.Models.EventDto.Response;
using KoiCoi.Modules.Repository.ChangePassword;
using KoiCoi.Modules.Repository.UserFeature;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
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

    public async Task<Result<string>> UploadEventAttachFile(EventPhotoPayload payload, int LoginUserId)
    {
        Result<string> result = null;
        try
        {
            if(!string.IsNullOrEmpty(payload.base64image) && !string.IsNullOrEmpty(payload.eventPostIdval)
                && !string.IsNullOrEmpty(payload.ext))
            {
                int PostId = Convert.ToInt32(Encryption.DecryptID(payload.eventPostIdval, LoginUserId.ToString()));
                var kcevent = await _db.Posts.Where(x=> x.PostId == PostId).FirstOrDefaultAsync();
                if (kcevent is not null)
                {
                       string bucketname = _configuration.GetSection("Buckets:EventImages").Get<string>()!;

                        string uniquekey = Globalfunction.NewUniqueFileKey(payload.ext!);
                        await _kcAwsS3Service.CreateFileAsync(payload.base64image!, bucketname, uniquekey, payload.ext!);
                        var newImage = new EventFile
                        {
                            Url = uniquekey,
                            UrlDescription = payload.Description,
                            EventPostId = kcevent.PostId,
                            CreatedDate = DateTime.UtcNow,
                            ModifiedDate = DateTime.UtcNow,
                            Extension = payload.ext,
                        };
                        await _db.EventFiles.AddAsync(newImage);
                        await _db.SaveChangesAsync();
                    result = Result<string>.Success("Upload Success");
                }
                else
                {
                    result = Result<string>.Error("Event Not Found");
                }
            }
            else
            {
                result = Result<string>.Error("Image Not Null or Event Not Null");
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
                bool currentDate = dateTime.Year == now.Year && dateTime.Month == now.Month;
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
                                              status == "active" ?
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
                                  select new
                                  {
                                      EventPostId = _eventMark.EventPostId,
                                      Mark = new EventMarks
                                      {
                                          MarkIdval = Encryption.EncryptID(_eventMark.MarkId.ToString(), LoginUserId.ToString()),
                                          IsoCode = _mark.Isocode,
                                          MarkName = _mark.MarkName,
                                          AllowedMarkName = _evall.AllowedMarkName,
                                          TotalBalance = Globalfunction.StringToDecimal(Encryption.DecryptID(_eventMark.TotalBalance.ToString(), balanceSalt)),
                                          LastBalance = Globalfunction.StringToDecimal(Encryption.DecryptID(_eventMark.LastBalance.ToString(), balanceSalt)),
                                          TargetBalance = _eventMark.TargetBalance != null ? Globalfunction.StringToDecimal(Encryption.DecryptID(_eventMark.TargetBalance.ToString(), balanceSalt)) : null
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

                                ///next check event mark balance
                                var evbalance = await _db.EventMarkBalances.Where(x => x.MarkId == MarkId && x.EventPostId == EventPostId).FirstOrDefaultAsync();
                                if(evbalance is null)
                                {
                                    string TotalBalance = Encryption.EncryptID("0.0", balanceSalt);
                                    string LastBalance = Encryption.EncryptID("0.0", balanceSalt);
                                    string? TargetBalance = item.TargetBalance != null ? Encryption.EncryptID(item.TargetBalance.Value.ToString(), balanceSalt) : null;
                                    EventMarkBalance newBalance = new EventMarkBalance { 
                                        EventPostId = EventPostId,
                                        MarkId = MarkId,
                                        TotalBalance=TotalBalance,
                                        LastBalance = LastBalance,
                                        TargetBalance = TargetBalance
                                    };
                                    await _db.EventMarkBalances.AddAsync(newBalance);
                                    await _db.SaveChangesAsync();
                                }

                                ///also check channel mark balance
                                int ChannelId = await _db.Events.Where(x => x.PostId == EventPostId).Select(x => x.ChannelId).FirstOrDefaultAsync();
                                if(ChannelId > 0)
                                {
                                    var chanBalance = await _db.ChannelMarkBalances.Where(x => x.ChannelId == ChannelId && x.MarkId == MarkId).FirstOrDefaultAsync();
                                    if(chanBalance is null)
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

    public async Task<Result<Pagination>> GetAllowedMarks(GetAllowedMarkPayload payload, int LoginUserId)
    {
        Result<Pagination> result = null;
        try
        {
            int EventPostId = Convert.ToInt32(Encryption.DecryptID(payload.EventIdval!, LoginUserId.ToString()));
            if (string.IsNullOrEmpty(payload.MarkTypeIdval) || payload.MarkTypeIdval.ToLower() == "all")
            {
                var allowedMark = await (from _evmk in _db.EventAllowedMarks
                                         join _mark in _db.Marks on _evmk.MarkId equals _mark.MarkId
                                         where _evmk.EventPostId == EventPostId
                                         select new
                                         {
                                             MarkIdval = Encryption.EncryptID(_mark.MarkId.ToString(), LoginUserId.ToString()),
                                             IsoCode = _mark.Isocode,
                                             AllowedMarkName = _evmk.AllowedMarkName,
                                         }).ToListAsync();

                Pagination pagination = RepoFunService.getWithPagination(payload.PageNumber, payload.PageSize, allowedMark);
                result = Result<Pagination>.Success(pagination);
            }
            else
            {
                int MarkTypeId = Convert.ToInt32(Encryption.DecryptID(payload.MarkTypeIdval!, LoginUserId.ToString()));
                var allowedMark = await (from _evmk in _db.EventAllowedMarks
                                         join _mark in _db.Marks on _evmk.MarkId equals _mark.MarkId
                                         join _mt in _db.MarkTypes on _mark.MarkTypeId equals _mt.MarkTypeId
                                         where _evmk.EventPostId == EventPostId && _mt.MarkTypeId == MarkTypeId
                                         select new
                                         {
                                             MarkIdval = Encryption.EncryptID(_mark.MarkId.ToString(), LoginUserId.ToString()),
                                             Isocode = _mark.Isocode,
                                             AllowedMarkName = _evmk.AllowedMarkName,
                                         }).ToListAsync();

                Pagination pagination = RepoFunService.getWithPagination(payload.PageNumber, payload.PageSize, allowedMark);
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
                                              where _cm.UserId == LoginUserId &&
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
                               where _ev.PostId == EventPostId && _colBal.MarkId == MarkId
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
}
