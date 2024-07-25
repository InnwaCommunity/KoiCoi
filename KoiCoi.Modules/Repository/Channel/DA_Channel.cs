using Azure;
using KoiCoi.Models.Via;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KoiCoi.Modules.Repository.Channel;

public class DA_Channel
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _configuration;

    public DA_Channel(AppDbContext db, IConfiguration configuration)
    {
        _db = db;
        _configuration = configuration;
    }

    public async Task<Result<string>> CreateChannelType(ViaChannelType viaChanType)
    {
        Result<string> model = null;
        try
        {
            await _db.ChannelTypes.AddAsync(viaChanType.ChangeChannelType());
            int result = await _db.SaveChangesAsync();
            if (result < 1) return Result<string>.Error("Save Channel Type Fail");

            model = Result<string>.Success("Save Channel Type Success");
        }
        catch (Exception ex)
        {
            model = Result<string>.Error(ex);
        }

        return model;
    }

    public async Task<Result<string>> UpdateChannelType(ChannelTypePayloads channelType, int ChannelTypeid)
    {
        Result<string> model = null;
        try
        {
            if (string.IsNullOrEmpty(channelType.ChannelTypeName) || string.IsNullOrEmpty(channelType.ChannelTypeDescription))
                return Result<string>.Error("Channel Name and Channel Description Can't Empty");
            var chanTypeRes = await _db.ChannelTypes
                                        .Where(x=> x.ChannelTypeId == ChannelTypeid)
                                        .FirstOrDefaultAsync();
            if (chanTypeRes is null)
                return Result<string>.Error("Channel Type Not Found");

            chanTypeRes.ChannelTypeName = channelType.ChannelTypeName!;
            chanTypeRes.ChannelTypeDescription = channelType.ChannelTypeDescription!;
            await _db.SaveChangesAsync();

            model = Result<string>.Success("Channel Type Update Success");
        }
        catch (Exception ex)
        {
            model = Result<string>.Error(ex);
        }

        return model;
    }
    public async Task<Result<string>> DeleteChannelType(int channelTypeId)
    {
        Result<string> model = null;
        try
        {
            var chanTyperes = await _db.ChannelTypes.Where(x=> x.ChannelTypeId==channelTypeId)
                .FirstOrDefaultAsync();
            if (chanTyperes is null) return Result<string>.Error("Channel Type Not Found");

           _db.ChannelTypes.Remove(chanTyperes);
            await _db.SaveChangesAsync();

            model = Result<string>.Success("Delete Success");

        }
        catch (Exception ex)
        {
            model = Result<string>.Error(ex);
        }
        return model;
    }
    public async Task<Result<List<ChannelTypeResponseDto>>> GetChannelType(int loginUserid)
    {
        Result<List<ChannelTypeResponseDto>> model = null;
        try
        {
            var chaltypelst = await _db.ChannelTypes
                                    .Select(x=> new ChannelTypeResponseDto
                                    {
                                        ChannelTypeIdval = Encryption.EncryptID(x.ChannelTypeId.ToString(), loginUserid.ToString()),
                                        ChannelTypeName = x.ChannelTypeName,
                                        ChannelTypeDescription = x.ChannelTypeDescription,
                                    })
                                    .ToListAsync();
            if (chaltypelst == null)
                return Result<List<ChannelTypeResponseDto>>.Error("Retrieve Channel Type Error");
            model = Result<List<ChannelTypeResponseDto>>.Success(chaltypelst);
        }
        catch (Exception ex)
        {
            model = Result<List<ChannelTypeResponseDto>>.Error(ex);
        }

        return model;
    }

    public async Task<Result<List<CurrencyResponseDto>>> GetCurrencyList(int LoginUserId)
    {
        Result<List<CurrencyResponseDto>> model = null;
        try
        {
            var currres = await _db.Currencies.ToListAsync();
            if (currres == null)
                throw new ValidationException("Channel Type Not Found");
            List<CurrencyResponseDto> currData= new List<CurrencyResponseDto>();
            foreach (var item in currres)
            {
                CurrencyResponseDto newres = new CurrencyResponseDto
                {
                    CurrencyIdval = Encryption.EncryptID(item.CurrencyId.ToString(), LoginUserId.ToString()),
                    CurrencyName = item.CurrencyName,
                    CurrencySymbol = item.CurrencySymbol,
                    IsoCode = item.IsoCode,
                    FractionalUnit = item.FractionalUnit,
                };
                currData.Add(newres);

            }

            model = Result<List<CurrencyResponseDto>>.Success(currData);
        }
        catch (Exception ex)
        {
            model =Result<List<CurrencyResponseDto>>.Error(ex);
        }
        return model;
    }

    public async Task<Result<ChannelDataResponse>> CreateChannel(CreateChannelReqeust channelReqeust, int LoginUserId,string filename)
    {
        Result<ChannelDataResponse> model = null;
        try
        {
            int ChanneltypeId = Convert.ToInt32(
                                Encryption.DecryptID(channelReqeust.ChannelTypeval!,
                                LoginUserId.ToString()));
            int CurrencyId = Convert.ToInt32(
                                Encryption.DecryptID(channelReqeust.CurrencyIdval!,
                                LoginUserId.ToString()));

            ViaChannel channel = new ViaChannel();
            channel.ChannelName = channelReqeust.ChannelName;
            channel.StatusDescription = channelReqeust.StatusDescription;
            channel.ChannelType = ChanneltypeId;
            channel.CreatorId = LoginUserId;
            channel.CurrencyId = CurrencyId;

            var addedChannel = (await _db.Channels.AddAsync(channel.ChangeChannel())).Entity;
            int result = await _db.SaveChangesAsync();
            if (result < 1) return Result<ChannelDataResponse>.Error("Create Channel Fail");

            ///Create MemberShip
            int? ownerid = await _db.UserTypes
                   .Where(x => x.Name == "owner")
                   .Select(x => x.TypeId)
                   .FirstOrDefaultAsync();
            int? approvedstatusId = await _db.StatusTypes
                    .Where(x=> x.StatusName == "Approved")
                    .Select(x=> x.StatusId) .FirstOrDefaultAsync();
            if (ownerid == null) return Result<ChannelDataResponse>.Error("Owner UserType Not Found");

            if (approvedstatusId == null) return Result<ChannelDataResponse>.Error("Approved Staus Not Found");
            
            ViaChannelMemberShip newViaChanMeShip = new ViaChannelMemberShip
            {
                ChannelId = addedChannel.ChannelId,
                UserId = LoginUserId,
                UserTypeId = ownerid.Value,
                StatusId = approvedstatusId.Value
            };
            await _db.ChannelMemberships.AddAsync(newViaChanMeShip.ChangeChannMemberShip());
            await _db.SaveChangesAsync();

            ///Save Profile Image
            ViaChannelProfile  viaChannelProfile = new ViaChannelProfile();
            viaChannelProfile.Url = filename;
            viaChannelProfile.UrlDescription = channelReqeust.imagedescription;
            viaChannelProfile.ChannelId = addedChannel.ChannelId;

            await _db.ChannelProfiles.AddAsync(viaChannelProfile.ChangeChannelProfile());
            await _db.SaveChangesAsync();

            ChannelDataResponse? data = await (from _cha in _db.Channels
                                               join ct in _db.ChannelTypes on _cha.ChannelType equals ct.ChannelTypeId
                                               join cur in _db.Currencies on _cha.CurrencyId equals cur.CurrencyId
                                               where _cha.ChannelId == addedChannel.ChannelId
                                               select new ChannelDataResponse
                                               {
                                                   ChannelIdval = Encryption.EncryptID(_cha.ChannelId.ToString(), LoginUserId.ToString()),
                                                   ChannelName = _cha.ChannelName,
                                                   ChannelDescription = _cha.StatusDescription,
                                                   ChannelType = ct.ChannelTypeName,
                                                   ISOCode = cur.IsoCode,
                                                   MemberCount = _cha.MemberCount,
                                                   TotalBalance = 0,
                                                   LastBalance = 0,
                                               }).FirstOrDefaultAsync();
            if (data is null) return Result<ChannelDataResponse>.Error("Channel Not Found");
            return Result<ChannelDataResponse>.Success(data);
        }
        catch (Exception ex)
        {
            return Result<ChannelDataResponse>.Error(ex);
        }
    }

    public async Task<Result<List<ChannelDataResponse>>> GetChannelsList(int LoginUserId)
    {
        Result<List<ChannelDataResponse>> model = null;
        try
        {
            int? ApprovedStatus = await _db.StatusTypes.Where(x=> x.StatusName.ToLower() == "approved")
                            .Select(x=> x.StatusId)
                            .FirstOrDefaultAsync();
            if (ApprovedStatus == null) return Result<List<ChannelDataResponse>>.Error("Status Type Not Found");
            List<ChannelDataResponse> Channels = await ( from _channel in _db.Channels 
                                               join _chantype in _db.ChannelTypes on _channel.ChannelType equals _chantype.ChannelTypeId
                                               join _curr in _db.Currencies on _channel.CurrencyId equals _curr.CurrencyId
                                               join _mem in _db.ChannelMemberships on _channel.ChannelId equals _mem.ChannelId
                                               where _mem.UserId == LoginUserId && _mem.StatusId == ApprovedStatus
                                                         orderby _channel.ChannelName
                                               select new ChannelDataResponse
                                               {
                                                   ChannelIdval = Encryption.EncryptID(_channel.ChannelId.ToString(), LoginUserId.ToString()),
                                                   ChannelName= _channel.ChannelName,
                                                   ChannelDescription = _channel.StatusDescription,
                                                   ChannelType = _chantype.ChannelTypeName,
                                                   MemberCount = _channel.MemberCount,
                                               }).ToListAsync();

            model= Result<List<ChannelDataResponse>>.Success(Channels);
        }
        catch (Exception ex)
        {
            model = Result<List<ChannelDataResponse>>.Error(ex);
        }
        return model;
    }

    public async Task<Result<string>> GetChannelProfile(int ChannelId,string destDir)
    {
        Result<string> model = null;
        try
        {
            var channel = await _db.Channels.Where(x => x.ChannelId == ChannelId).FirstOrDefaultAsync();
            if (channel is null) throw new ValidationException("Channel Not Found");

            var profiles = await _db.ChannelProfiles
                       .Where(x => x.ChannelId == ChannelId)
                       .OrderByDescending(x => x.ProfileId) // Order by Id in descending order
                       .FirstOrDefaultAsync();
            if (profiles is null)
            {
                model = Result<string>.Success("");
            }
            else
            {
                string profileimg = Path.Combine(destDir, profiles.Url);
                if (!System.IO.File.Exists(profileimg))
                {
                    model = Result<string>.Success("");
                }
                else
                {
                    byte[] imageBytes = System.IO.File.ReadAllBytes(profileimg);
                    string base64String = Convert.ToBase64String(imageBytes);
                    model = Result<string>.Success(base64String);
                }
            }
        }
        catch (Exception ex)
        {
            model = Result<string>.Error(ex);
        }
        return model;
    }

    public async Task<Result<string>> UploadProfile(int LoginUserId, int ChannelId,string filename,string? description)
    {
        Result<string> model = null;

        try
        {
            var channel = await _db.Channels.Where(x => x.ChannelId == ChannelId)
                                            .FirstOrDefaultAsync();

            if (channel is null) return Result<string>.Error("Channel Not Found");
            ViaChannelProfile viaChannelProfile = new ViaChannelProfile();
            viaChannelProfile.ChannelId = ChannelId;
            viaChannelProfile.Url = filename;
            viaChannelProfile.UrlDescription = description;
            await _db.ChannelProfiles.AddAsync(viaChannelProfile.ChangeChannelProfile());
            await _db.SaveChangesAsync();
            model = Result<string>.Success("Success");

        }
        catch (Exception ex)
        {
            model = Result<string>.Error(ex);
        }
        return model;
    }

    public async Task<Result<string>> GenerateChannelUrl(int ChannelId,int LoginUserId)
    {
        Result<string> model = null;
        try
        {
            string domainUrl = _configuration["appSettings:DomainUrl"] ?? throw new Exception("Invalid DomainUrl");
            string urlSalt = _configuration["appSettings:UrlSalt"] ?? throw new Exception("Invalid UrlSalt");

            var resda = await _db.Channels.Where(x => x.ChannelId == ChannelId).FirstOrDefaultAsync();
            if (resda is null) return Result<string>.Error("Channel Not Found");

            string channeldata = $"{LoginUserId}/{ChannelId}";
            string encryptdata = Encryption.EncryptID(channeldata, urlSalt);
            string url = domainUrl +"Channel/" + encryptdata;
            model = Result<string>.Success(url);

        }
        catch (Exception ex)
        {
            model = Result<string>.Error(ex);
        }
        return model;
    }

    public async Task<Result<VisitChannelResponse>> VisitChannelByInviteLink(string inviteLink,int LoginUserId)
    {
        Result<VisitChannelResponse> model = null;
        try
        {
            string urlSalt = _configuration["appSettings:UrlSalt"] ?? throw new Exception("Invalid UrlSalt"); 
            string balanceSalt = _configuration["appSettings:BalanceSalt"] ?? throw new Exception("Invalid Balance Salt");
            string desdata=Encryption.DecryptID(inviteLink, urlSalt);
            string[] splidata = desdata.Split('/');
            int inviterId = Convert.ToInt32(splidata[0]);
            int channelId = Convert.ToInt32(splidata[1]);
            var IsMember = await _db.ChannelMemberships
                                    .Where(x => x.UserId == LoginUserId && x.ChannelId == channelId)
                                    .FirstOrDefaultAsync();
            if(IsMember is null)
            {

                VisitChannelResponse? reData = await (from ch in _db.Channels
                                                      join ct in _db.ChannelTypes on ch.ChannelType equals ct.ChannelTypeId
                                                      join user in _db.Users on ch.CreatorId equals user.UserId
                                                      join cr in _db.Currencies on ch.CurrencyId equals cr.CurrencyId
                                                      where ch.ChannelId == channelId
                                                      select new VisitChannelResponse
                                                      {
                                                          ChannelIdval = Encryption.EncryptID(ch.ChannelId.ToString(), LoginUserId.ToString()),
                                                          ChannelName = ch.ChannelName,
                                                          ChannelDescription = ch.StatusDescription,
                                                          IsMember = false,
                                                          MemberStatus = null,
                                                          ChannelType = ct.ChannelTypeName,
                                                          CreatorIdval = Encryption.EncryptID(user.UserId.ToString(), LoginUserId.ToString()),
                                                          CreatorName = user.Name,
                                                          MemberCount = ch.MemberCount,
                                                          TotalBalance = null,
                                                          LastBalance = null,


                                                      }).FirstOrDefaultAsync();

                if (reData is null) return Result<VisitChannelResponse>.Error("Channel Not Found");

                var visitRecord = await _db.VisitChannelHistories.Where(
                                    x => x.UserId == LoginUserId
                                    && x.InviterId == inviterId
                                    && x.ChannelId == channelId).FirstOrDefaultAsync();
                if(visitRecord is null)
                {
                    VisitChannelHistory inviteHist = new VisitChannelHistory
                    {
                        UserId = LoginUserId,
                        InviterId = inviterId,
                        ChannelId = channelId,
                        ViewedDate = DateTime.Now
                    };
                    await _db.VisitChannelHistories.AddAsync(inviteHist);
                }
                else
                {
                    visitRecord.ViewedDate = DateTime.Now;
                    _db.VisitChannelHistories.Update(visitRecord);
                }
                await _db.SaveChangesAsync();
                model = Result<VisitChannelResponse>.Success(reData);
            }
            else
            {

                VisitChannelResponse? reData = await (from ch in _db.Channels
                                    join ct in _db.ChannelTypes on ch.ChannelType equals ct.ChannelTypeId
                                    join user in _db.Users on ch.CreatorId equals user.UserId
                                    join cr in _db.Currencies on ch.CurrencyId equals cr.CurrencyId
                                    where ch.ChannelId == channelId
                                    select new VisitChannelResponse
                                    {
                                        ChannelIdval = Encryption.EncryptID(ch.ChannelId.ToString(), LoginUserId.ToString()),
                                        ChannelName = ch.ChannelName,
                                        ChannelDescription = ch.StatusDescription,
                                        ChannelType = ct.ChannelTypeName,
                                        CreatorIdval = Encryption.EncryptID(user.UserId.ToString(), LoginUserId.ToString()),
                                        CreatorName = user.Name,
                                        ISOCode = cr.IsoCode,
                                        MemberCount = ch.MemberCount,
                                        TotalBalance=Globalfunction.StringToDecimal(ch.TotalBalance == "0" || ch.TotalBalance == null ? "0" :
                                                            Encryption.DecryptID(ch.TotalBalance.ToString(), balanceSalt)),
                                        LastBalance = Globalfunction.StringToDecimal(ch.LastBalance == "0" || ch.LastBalance == null ? "0" :
                                                           Encryption.DecryptID(ch.LastBalance.ToString(), balanceSalt)),
                                        
                                    }).FirstOrDefaultAsync();

                if (reData is null) return Result<VisitChannelResponse>.Error("Channel Not Found");
                var statusdata = await (from cm in _db.ChannelMemberships
                                        join st in _db.StatusTypes on cm.StatusId equals st.StatusId
                                        where cm.ChannelId == channelId && cm.UserId == LoginUserId
                                        select new
                                        {
                                            StatusId = st.StatusId,
                                            StatusName = st.StatusName,
                                        }).FirstOrDefaultAsync();
                if (statusdata is null) return Result<VisitChannelResponse>.Error("Member Status Not Found");
                reData.IsMember = statusdata.StatusName.ToLower() == "approved";
                reData.MemberStatus = statusdata.StatusName;
                model = Result<VisitChannelResponse>.Success(reData);
            }

        }
        catch (Exception ex)
        {
            model = Result<VisitChannelResponse>.Error(ex);
        }
        return model;
    }

    public async Task<Result<string>> JoinChannelByInviteLink(JoinChannelInviteLinkPayload payload, int LoginUserId)
    {
        Result<string> model = null;
        try
        {
            string urlSalt = _configuration["appSettings:UrlSalt"] ?? throw new Exception("Invalid UrlSalt");
            string desdata = Encryption.DecryptID(payload.InviteLink!, urlSalt);
            string[] splidata = desdata.Split('/');
            int inviterId = Convert.ToInt32(splidata[0]);
            int channelId = Convert.ToInt32(splidata[1]);

            ///Join
            if (payload.IsJoin ?? true)
            {
                var IsMember = await _db.ChannelMemberships
                                        .Where(x => x.UserId == LoginUserId && x.ChannelId == channelId)
                                        .FirstOrDefaultAsync();
                if (IsMember is not null) return Result<string>.Error("Already Joined");

                var hasChannel = await _db.Channels.Where(x => x.ChannelId == channelId).FirstOrDefaultAsync();
                if (hasChannel is null) return Result<string>.Error("Channel Not Found");
                int? memberLevel = await _db.UserTypes.Where(x => x.Name.ToLower() == "member").Select(x => x.TypeId).FirstOrDefaultAsync();
                if (memberLevel is null) return Result<string>.Error("Member User Type Not Found");
                ChannelMembership meship = new ChannelMembership
                {
                    ChannelId = channelId,
                    UserId = LoginUserId,
                    UserTypeId = memberLevel.Value,
                    StatusId = 1,
                    JoinedDate = DateTime.Now,
                    InviterId = inviterId,
                };
                await _db.ChannelMemberships.AddAsync(meship);
                await _db.SaveChangesAsync();
                model = Result<string>.Success("Joined Success");
            }
            else
            {
                ///Cancel
                var isReqeust = await _db.ChannelMemberships
                                        .Where(x => x.UserId == LoginUserId
                                        && x.ChannelId == channelId)
                                        .FirstOrDefaultAsync();
                if (isReqeust is not null)
                {
                    if (isReqeust.StatusId == 1 || isReqeust.StatusId == 3)
                    {
                        _db.ChannelMemberships.Remove(isReqeust);
                        await _db.SaveChangesAsync();
                        model = Result<string>.Success("Cancel Success");
                    }
                    else
                    {
                        model = Result<string>.Error("You are already member,so you can leave in channel detail");
                    }
                }
                else
                {
                    model = Result<string>.Error("You are already cancel");
                }
            }
        }
        catch (Exception ex)
        {
            model = Result<string>.Error(ex);
        }
        return model;
    }

    public async Task<Result<List<ChannelMemberResponse>>> GetChannelMember(string ChannelIdval,string MemberStatus, int LoginUserId)
    {
        Result<List<ChannelMemberResponse>> model = null;
        try
        {
            int channelId = Convert.ToInt32(Encryption.DecryptID(ChannelIdval, LoginUserId.ToString()));
            var chan = await _db.Channels.Where(x=> x.ChannelId == channelId).FirstOrDefaultAsync();
            var statusType = await _db.StatusTypes.Where(x=> x.StatusName.ToLower() == MemberStatus.ToLower()).FirstOrDefaultAsync();


            if (chan is null) return Result<List<ChannelMemberResponse>>.Error("Channel Not Found");
            if(statusType is null) return Result<List<ChannelMemberResponse>>.Error("User Type Not Found");
            string baseDirectory = _configuration["appSettings:UploadPath"] ?? throw new Exception("Invalid UploadPath");
            string uploadDirectory = _configuration["appSettings:UserProfile"] ?? throw new Exception("Invalid function upload path.");
            string destDirectory = Path.Combine(baseDirectory, uploadDirectory);


            var loginUserType = await (from ch in _db.ChannelMemberships
                                              join ut in _db.UserTypes on ch.UserTypeId equals ut.TypeId
                                              where ch.ChannelId == channelId && ch.UserId == LoginUserId
                                              select new
                                              {
                                                  UserTypeName = ut.Name
                                              }).FirstOrDefaultAsync();
            if(loginUserType is null || loginUserType.UserTypeName.ToLower()=="member" )
                return Result<List<ChannelMemberResponse>>.Success(new List<ChannelMemberResponse>());


            List<ChannelMemberResponse> query = await (from ch in _db.ChannelMemberships
                                                       join us in _db.Users on ch.UserId equals us.UserId
                                                       join inv in _db.Users on ch.InviterId equals inv.UserId into invGroup
                                                       from inv in invGroup.DefaultIfEmpty()
                                                       join usertype in _db.UserTypes on ch.UserTypeId equals usertype.TypeId
                                                       where ch.ChannelId == channelId && ch.StatusId == statusType.StatusId
                                                       select new ChannelMemberResponse
                                                       {
                                                           MembershipId = Encryption.EncryptID(ch.MembershipId.ToString(), LoginUserId.ToString()),
                                                           MemberIdval = ch.UserId.ToString(),
                                                           MemberName = us.Name,
                                                           UserTypeIdval = Encryption.EncryptID(usertype.TypeId.ToString(), LoginUserId.ToString()),
                                                           UserTypeName = usertype.Name,
                                                           InviterIdval = inv != null ? inv.UserId.ToString() : null,
                                                           InviterName = inv != null ? inv.Name : null,
                                                           JoinedDate = ch.JoinedDate,
                                                           UserImage64 = ""
                                                       }).ToListAsync();

            /* List<ChannelMemberResponse> query = await (from ch in _db.ChannelMemberships
                                join us in _db.Users on ch.UserId equals us.UserId
                                join inv in _db.Users on ch.InviterId equals inv.UserId
                                join usertype in _db.UserTypes on ch.UserTypeId equals usertype.TypeId
                                //join pro in _db.UserProfiles on ch.UserId equals pro.UserId
                                where ch.ChannelId == channelId && ch.StatusId == statusType.StatusId
                                 select new ChannelMemberResponse
                                {
                                     MembershipId = Encryption.EncryptID(ch.MembershipId.ToString(), LoginUserId.ToString()),
                                     MemberIdval = ch.UserId.ToString(), 
                                    MemberName = us.Name,
                                     UserTypeIdval = Encryption.EncryptID(usertype.TypeId.ToString(), LoginUserId.ToString()),
                                     UserTypeName = usertype.Name,
                                     InviterIdval = inv.UserId.ToString(),
                                    InviterName = inv.Name,
                                    JoinedDate = ch.JoinedDate,
                                     UserImage64 = ""
                                 }).ToListAsync();
             */
            List<ChannelMemberResponse> resData = new List<ChannelMemberResponse>();

            foreach( ChannelMemberResponse response in query )
            {
                /*string? userImageUrl = await _db.UserProfiles.Where(x => x.UserId == Convert.ToInt32(response.MemberIdval))
                                        .Select(x=> x.Url)
                                        .FirstOrDefaultAsync();
                if (!string.IsNullOrEmpty(userImageUrl))
                {
                    string profileimg = Path.Combine(destDirectory, userImageUrl);
                    byte[] imageBytes = System.IO.File.ReadAllBytes(profileimg);
                    string base64String = Convert.ToBase64String(imageBytes);
                    response.UserImage64 = base64String;
                }*/
                response.MemberIdval = Encryption.EncryptID(response.MemberIdval!, LoginUserId.ToString());
                response.InviterIdval = Encryption.EncryptID(response.InviterIdval!, LoginUserId.ToString());
                resData.Add(response);
            }
            model = Result<List<ChannelMemberResponse>>.Success(resData);
        }
        catch (Exception ex)
        {
            model = Result<List<ChannelMemberResponse>>.Error(ex);
        }
        return model;
    }
    public async Task<Result<string>> ApproveRejectChannelMember(List<AppRejChannelMemberPayload> payload, int LoginUserId)
    {
        Result<string> model = null;
        try
        {
            foreach (var item in payload)
            {
                if (!string.IsNullOrEmpty(item.MembershipIdval) && !string.IsNullOrEmpty(item.UserTypeIdval))
                {
                    int MembershipId = Convert.ToInt32(Encryption.DecryptID(item.MembershipIdval, LoginUserId.ToString()));
                    int userTypeId = Convert.ToInt32(Encryption.DecryptID(item.UserTypeIdval, LoginUserId.ToString()));

                    ///1 to approve
                    if (item.ApproveStatus == 1)
                    {
                        int ApproveStatus = await _db.StatusTypes.Where(x => x.StatusName.ToLower() == "approved")
                            .Select(x => x.StatusId).FirstOrDefaultAsync();
                        var membership = await _db.ChannelMemberships.Where(x => x.MembershipId == MembershipId)
                                                .FirstOrDefaultAsync();
                        if (membership is not null && membership.StatusId != ApproveStatus)
                        {
                            membership.StatusId = ApproveStatus;
                            membership.UserTypeId = userTypeId;
                            _db.ChannelMemberships.Update(membership);
                            await _db.SaveChangesAsync();

                            //update channel member count
                            var channel = await _db.Channels.Where(x => x.ChannelId == membership.ChannelId)
                                                .FirstOrDefaultAsync();

                            if (channel is not null)
                            {
                                channel.MemberCount = channel.MemberCount + 1;
                                _db.Channels.Update(channel);
                                await _db.SaveChangesAsync();
                            }
                        }
                    }
                    ///2 to reject
                    else if(item.ApproveStatus == 2)
                    {
                        int RejectStatus = await _db.StatusTypes.Where(x => x.StatusName.ToLower() == "rejected")
                            .Select(x => x.StatusId).FirstOrDefaultAsync();
                        var membership = await _db.ChannelMemberships.Where(x => x.MembershipId == MembershipId)
                                                .FirstOrDefaultAsync();
                        if (membership is not null)
                        {
                            membership.StatusId = RejectStatus;
                            _db.ChannelMemberships.Update(membership);
                            await _db.SaveChangesAsync();
                        }
                    }
                }
            }
            model = Result<string>.Success("Success");
        }
        catch (Exception ex)
        {
            model = Result<string>.Error(ex);
        }
        return model;
    }

    public async Task<Result<VisitUserResponse>> GetVisitUsersRecords(GetVisitUsersPayload payload, int LoginUserId)
    {
        Result<VisitUserResponse> model = null;
        return model;
    }
}
