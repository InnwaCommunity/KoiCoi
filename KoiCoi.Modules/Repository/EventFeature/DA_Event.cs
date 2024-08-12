
using KoiCoi.Database.AppDbContextModels;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace KoiCoi.Modules.Repository.EventFreture;

public class DA_Event
{
    private readonly AppDbContext _db;
    private readonly NotificationManager.NotificationManager _saveNotifications;
    private readonly IConfiguration _configuration;

    public DA_Event(AppDbContext db, IConfiguration configuration, NotificationManager.NotificationManager saveNotifications)
    {
        _db = db;
        _configuration = configuration;
        _saveNotifications = saveNotifications;
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
            int CurrencyId = await _db.Channels.Where(x=> x.ChannelId == ChannelId)
                .Select(x=> x.CurrencyId)
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
            string TotalBalance = Encryption.EncryptID("0.0", balanceSalt);
            string LastBalance = Encryption.EncryptID("0.0", balanceSalt);
            string? TargetBalance = paylod.TargetBalance != null ? Encryption.EncryptID(paylod.TargetBalance.ToString()!, balanceSalt) : null;
            Event newEvent = new Event
            {
                EventName = paylod.EventName!,
                EventDescription = paylod.EventDescription,
                ChannelId = ChannelId,
                CreatorId = LoginUserId,
                ApproverId = ownerusertype is not null ? LoginUserId : null,
                StatusId = status,
                CurrencyId = CurrencyId,
                TotalBalance = TotalBalance,
                LastBalance = LastBalance,
                TargetBalance = TargetBalance,
                StartDate = DateTime.Parse(paylod!.StartDate!),
                EndDate = DateTime.Parse(paylod!.EndDate!),
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow,
                Inactive = false
            };
            var res = await _db.Events.AddAsync(newEvent);
            await _db.SaveChangesAsync();
            result = Result<string>.Success("Requested Event Success");
            if (paylod.EventAddresses.Any())
            {
                foreach (var address in paylod.EventAddresses)
                {
                    int AddressId = Convert.ToInt32(Encryption.EncryptID(address.AddressTypeIdval, LoginUserId.ToString()));
                    EventAddress newAddress = new EventAddress
                    {
                        AddressId = AddressId,
                        EventId = newEvent.Eventid,
                        AddressName = address.AddressName,
                    };
                    await _db.EventAddresses.AddAsync(newAddress);
                    await _db.SaveChangesAsync();
                }
            }
            if(paylod.EventPhotos.Any())
            {
                string baseDirectory = _configuration["appSettings:UploadPath"] ?? throw new Exception("Invalid UploadPath");
                string uploadDirectory = _configuration["appSettings:EventImages"] ?? throw new Exception("Invalid function upload path.");
                string destDirectory = Path.Combine(baseDirectory, uploadDirectory);
                if (!Directory.Exists(destDirectory))
                {
                    Directory.CreateDirectory(destDirectory);
                }
                foreach (var item in paylod.EventPhotos)
                {
                    string filename = Globalfunction.NewUniqueFileName() + ".png";
                    string base64Str = item.base64image!;
                    byte[] bytes = Convert.FromBase64String(base64Str!);

                    string filePath = Path.Combine(destDirectory, filename);
                    if (filePath.Contains(".."))
                    { //if found .. in the file name or path
                        Log.Error("Invalid path " + filePath);
                    }
                    await System.IO.File.WriteAllBytesAsync(filePath, bytes);

                    var newImage = new EventFile
                    {
                        Url = filename,
                        UrlDescription = item.Description,
                        EventId= newEvent.Eventid,
                        CreatedDate = DateTime.UtcNow,
                        ModifiedDate = DateTime.UtcNow,
                        Extension = "png",
                    }; 
                    await _db.EventFiles.AddAsync(newImage);
                    await _db.SaveChangesAsync();
                }
            }
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
                    $"UpcomingNewEvent/{newEvent.Eventid}");
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
                    $"RequestedNewEvent/{newEvent.Eventid}");
            }
        }
        catch (Exception ex)
        {
            result = Result<string>.Error(ex);
        }

        return result;
    }

    public async Task<Result<List<GetRequestEventResponse>>> GetEventRequestList(GetEventRequestPayload payload, int LoginUserId)
    {
        Result<List<GetRequestEventResponse>> result;
        try
        {
            int ChannelId = Convert.ToInt32(Encryption.DecryptID(payload.ChannelIdval!, LoginUserId.ToString()));
            string balanceSalt = _configuration["appSettings:BalanceSalt"] ?? throw new Exception("Invalid Balance Salt");
            string status = payload.Status!;
            var query = await (from _ev in _db.Events
                               join _cre in _db.Users on _ev.CreatorId equals _cre.UserId
                               join _sta in _db.StatusTypes on _ev.StatusId equals _sta.StatusId
                               join _cur in _db.Currencies on _ev.CurrencyId equals _cur.CurrencyId
                               join _meship in _db.ChannelMemberships on _ev.ChannelId equals _meship.ChannelId
                               join _usertype in _db.UserTypes on _meship.UserTypeId equals _usertype.TypeId
                               where _ev.ChannelId == ChannelId
                               && _sta.StatusName.ToLower() == status
                               && _ev.Inactive == false
                               && _meship.UserId == LoginUserId
                               && (status.ToLower() == "approved" || 
                               _usertype.Name.ToLower() == "owner" || 
                               _usertype.Name.ToLower() == "admin")
                               select new 
                               {
                                   EventIdval = _ev.Eventid,
                                   EventName = _ev.EventName,
                                   EventDescrition = _ev.EventDescription,
                                   CreatorIdval = _cre.UserId,
                                   Currency = _cur.IsoCode,
                                   CreatorName = _cre.Name,
                                   TotalBalance = Globalfunction.StringToDecimal(
                                       _ev.TotalBalance == "0" ||
                                       _ev.TotalBalance == null ? "0" : 
                                       Encryption.DecryptID(_ev.TotalBalance.ToString(), balanceSalt)),
                                   LastBalance = Globalfunction.StringToDecimal(
                                       _ev.LastBalance == "0" ||
                                       _ev.LastBalance == null ? "0" :
                                       Encryption.DecryptID(_ev.LastBalance.ToString(), balanceSalt)),
                                   StartDate = _ev.StartDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                                   EndDate = _ev.EndDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                                   ModifiedDate = _ev.ModifiedDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                               }).ToListAsync();
            List<GetRequestEventResponse> responseList = new List<GetRequestEventResponse>();
            foreach (var item in query)
            {
                var imgquery = await _db.EventFiles.Where(x => x.EventId == item.EventIdval)
                    .Select(x=> new EventImageInfo
                    {
                        imgfilename = x.Url,
                        imgDescription = x.UrlDescription
                    } )
                    .ToListAsync();
                GetRequestEventResponse newres= new GetRequestEventResponse
                {
                    EventIdval = Encryption.EncryptID(item.EventIdval.ToString(), LoginUserId.ToString()),
                    EventName = item.EventName,
                    EventDescrition = item.EventDescrition,
                    CreatorIdval = Encryption.EncryptID(item.CreatorIdval.ToString(), LoginUserId.ToString()),
                    CreatorName = item.CreatorName,
                    Currency = item.Currency,
                    TotalBalance = item.TotalBalance,
                    LastBalance = item.LastBalance,
                    StartDate = item.StartDate,
                    EndDate = item.EndDate,
                    ModifiedDate = item.ModifiedDate,
                    EventImageList = imgquery
                };
                responseList.Add(newres);
            }
            result = Result<List<GetRequestEventResponse>>.Success(responseList);
        }
        catch (Exception ex)
        {
            result = Result<List<GetRequestEventResponse>>.Error(ex);
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
                int EventId = Convert.ToInt32(Encryption.DecryptID(item.EventIdval!, LoginUserId.ToString()));
                var checkloginusertype = await (from _ev in _db.Events
                                                join _meme in _db.ChannelMemberships on _ev.ChannelId equals _meme.ChannelId
                                                join _usety in _db.UserTypes on _meme.UserTypeId equals _usety.TypeId
                                                where _ev.Eventid == EventId
                                                && _meme.UserId == LoginUserId
                                                && (_usety.Name.ToLower() == "owner" || _usety.Name.ToLower() == "admin")
                                                select _usety.Name)
                                                .FirstOrDefaultAsync();
                if (checkloginusertype is not null)
                {
                    var oldevent = await _db.Events.Where(x => x.Eventid == EventId).FirstOrDefaultAsync();
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
                                    EventId = oldevent.Eventid,
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
                                $"UpcomingNewEvent/{oldevent.Eventid}");
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
                                $"RejectedNewEvent/{oldevent.Eventid}");
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
            int EventId = Convert.ToInt32(Encryption.DecryptID(payload.EventIdval!, LoginUserId.ToString()));
            var checkLoginUserAccess = await (from meship in _db.EventMemberships
                                              join usertype in _db.UserTypes on meship.UserTypeId equals usertype.TypeId
                                              where meship.UserId == LoginUserId && meship.EventId == EventId
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
                        .Where(x => x.EventId == EventId && x.UserId == UserId)
                        .FirstOrDefaultAsync();
                    if (eventme is null)
                    {
                        eventme = new EventMembership
                        {
                            EventId = EventId,
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
                                        where _meme.EventId == EventId
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
                                      where _meme.EventId == EventId
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
            int EventId = Convert.ToInt32(Encryption.DecryptID(payload.EventIdval!, LoginUserId.ToString()));
            List<EventAdminsResponse> query = await (from _em in _db.EventMemberships
                               join _ut in _db.UserTypes on _em.UserTypeId equals _ut.TypeId
                               join _meb in _db.Users on _em.UserId equals _meb.UserId
                               join _ev in _db.Events on _em.EventId equals _ev.Eventid
                               join _ms in _db.ChannelMemberships on _ev.ChannelId equals _ms.ChannelId
                               join _logu in _db.Users on _ms.UserId equals _logu.UserId
                               where _em.EventId == EventId && _logu.UserId == LoginUserId
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
}
