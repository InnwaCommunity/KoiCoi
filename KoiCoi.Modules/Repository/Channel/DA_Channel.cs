using KoiCoi.Models.Via;
using Microsoft.Extensions.Configuration;
using System;
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

    public async Task<ResponseData> CreateChannelType(ViaChannelType viaChanType)
    {
        ResponseData responseData = new ResponseData();
        try
        {
            await _db.ChannelTypes.AddAsync(viaChanType.ChangeChannelType());
            int result = await _db.SaveChangesAsync();
            if (result < 1)
                throw new ValidationException("Save Channel Type Fail");

            responseData.StatusCode = 1;
            responseData.Message = "Save Channel Type Success";
            return responseData;
        }
        catch (ValidationException vex)
        {
            responseData.StatusCode = 0;
            responseData.Message = vex.ValidationResult.ErrorMessage;
            return responseData;
        }
        catch (Exception ex)
        {
            responseData.StatusCode = 0;
            responseData.Message = ex.Message;
            return responseData;
        }
    }

    public async Task<ResponseData> UpdateChannelType(ChannelTypePayloads channelType, int ChannelTypeid)
    {
        ResponseData responseData = new ResponseData();
        try
        {
            if (string.IsNullOrEmpty(channelType.ChannelTypeName) || string.IsNullOrEmpty(channelType.ChannelTypeDescription))
                throw new ValidationException("Channel Name and Channel Description Can't Empty");
            var chanTypeRes = await _db.ChannelTypes
                                        .Where(x=> x.ChannelTypeId == ChannelTypeid)
                                        .FirstOrDefaultAsync();
            if (chanTypeRes is null)
                throw new ValidationException("Channel Type Not Found");

            chanTypeRes.ChannelTypeName = channelType.ChannelTypeName!;
            chanTypeRes.ChannelTypeDescription = channelType.ChannelTypeDescription!;
            await _db.SaveChangesAsync();

            responseData.StatusCode = 1;
            responseData.Message = "Channel Type Update Success";
            return responseData;
        }
        catch (ValidationException vex)
        {
            responseData.StatusCode = 0;
            responseData.Message = vex.ValidationResult.ErrorMessage;
            return responseData;
        }
        catch (Exception ex)
        {
            responseData.StatusCode = 0;
            responseData.Message = ex.Message;
            return responseData;
        }
    }
    public async Task<ResponseData> DeleteChannelType(int channelTypeId)
    {
        ResponseData responseData = new ResponseData();
        try
        {
            var chanTyperes = await _db.ChannelTypes.Where(x=> x.ChannelTypeId==channelTypeId)
                .FirstOrDefaultAsync();
            if (chanTyperes is null) throw new ValidationException("Channel Type Not Found");

           _db.ChannelTypes.Remove(chanTyperes);
            await _db.SaveChangesAsync();
            responseData.StatusCode = 1;
            responseData.Message = "Delete Success";
            return responseData;

        }
        catch (ValidationException vex)
        {
            responseData.StatusCode = 0;
            responseData.Message = vex.ValidationResult.ErrorMessage;
            return responseData;
        }
        catch (Exception ex)
        {
            responseData.StatusCode = 0;
            responseData.Message = ex.Message;
            return responseData;
        }
    }
    public async Task<ResponseData> GetChannelType(int loginUserid)
    {
        ResponseData responseData = new ResponseData();
        try
        {
            var chaltypelst = await _db.ChannelTypes
                                    .ToListAsync();
            if(chaltypelst==null)
                throw new ValidationException("Retrieve Channel Type Error");
            List<ChannelTypeResponseDto> chaltype = new List<ChannelTypeResponseDto>();

            foreach (var item in chaltypelst)
            {
                ChannelTypeResponseDto newRes = new ChannelTypeResponseDto
                {
                    ChannelTypeIdval = Encryption.EncryptID(item.ChannelTypeId.ToString(), loginUserid.ToString()),
                    ChannelTypeName = item.ChannelTypeName,
                    ChannelTypeDescription = item.ChannelTypeDescription,
                };
                chaltype.Add(newRes);
            }
            responseData.StatusCode = 1;
            responseData.Message = "Retrieve Success";
            responseData.Data = chaltype;
            return responseData;
        }
        catch (ValidationException vex)
        {
            responseData.StatusCode = 0;
            responseData.Message = vex.ValidationResult.ErrorMessage;
            return responseData;
        }
        catch (Exception ex)
        {
            responseData.StatusCode = 0;
            responseData.Message = ex.Message;
            return responseData;
        }
    }

    public async Task<ResponseData> GetCurrencyList(int LoginUserId)
    {
        ResponseData responseData = new ResponseData();
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
            responseData.StatusCode = 1;
            responseData.Message = "Get Currency Success";
            responseData.Data = currData;
            return responseData;
        }
        catch (ValidationException vex)
        {
            responseData.StatusCode = 0;
            responseData.Message = vex.ValidationResult.ErrorMessage;
            return responseData;
        }
        catch (Exception ex)
        {
            responseData.StatusCode = 0;
            responseData.Message = ex.Message;
            return responseData;
        }
    }

    public async Task<ResponseData> CreateChannel(CreateChannelReqeust channelReqeust, int LoginUserId,string filename)
    {
        ResponseData responseData = new ResponseData();
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
            if (result < 1) throw new ValidationException("Create Channel Fail");


            if(addedChannel == null) throw new ValidationException("Create Channel Fail");

            ///Create MemberShip
            int? ownerid = await _db.UserTypes
                   .Where(x => x.Name == "owner")
                   .Select(x => x.TypeId)
                   .FirstOrDefaultAsync();
            int? approvedstatusId = await _db.StatusTypes
                    .Where(x=> x.StatusName == "Approved")
                    .Select(x=> x.StatusId) .FirstOrDefaultAsync();
            if(ownerid == null) throw new ValidationException("Owner UserType Not Found");

            if (approvedstatusId == null) throw new ValidationException("Approved Staus Not Found");
            ViaChannelMemberShip newViaChanMeShip = new ViaChannelMemberShip();
            newViaChanMeShip.ChannelId = addedChannel.ChannelId;
            newViaChanMeShip.UserId = LoginUserId;
            newViaChanMeShip.UserTypeId = ownerid.Value;
            newViaChanMeShip.StatusId = approvedstatusId.Value;
            await _db.ChannelMemberships.AddAsync(newViaChanMeShip.ChangeChannMemberShip());
            await _db.SaveChangesAsync();

            ///Save Profile Image
            ViaChannelProfile  viaChannelProfile = new ViaChannelProfile();
            viaChannelProfile.Url = filename;
            viaChannelProfile.UrlDescription = channelReqeust.imagedescription;
            viaChannelProfile.ChannelId = addedChannel.ChannelId;

            await _db.ChannelProfiles.AddAsync(viaChannelProfile.ChangeChannelProfile());
            await _db.SaveChangesAsync();

            responseData.StatusCode = 1;
            responseData.Message = "Create Channel Success";
            return responseData;
        }
        catch (ValidationException vex)
        {
            responseData.StatusCode = 0;
            responseData.Message = vex.ValidationResult.ErrorMessage;
            return responseData;
        }
        catch (Exception ex)
        {
            responseData.StatusCode = 0;
            responseData.Message = ex.Message;
            return responseData;
        }
    }

    public async Task<ResponseData> GetChannels(int LoginUserId)
    {
        ResponseData responseData = new ResponseData();
        try
        {
            int? ownerid = await _db.UserTypes
               .Where(x => x.Name == "owner")
               .Select(x => x.TypeId)
               .FirstOrDefaultAsync();
            if (ownerid == null) throw new ValidationException("Owner UserType Not Found");

            var ChannelsWithOwnerName = await (from _cm in _db.ChannelMemberships
                                               join _user in _db.Users on _cm.UserId equals _user.UserId
                                               join _channel in _db.Channels on _cm.ChannelId equals _channel.ChannelId
                                               where _cm.UserTypeId == ownerid
                                               orderby _channel.ChannelName
                                               select new
                                               {
                                                   UserIdval = Encryption.EncryptID(_user.UserId.ToString(), LoginUserId.ToString()),
                                                   OwnerName = _user.Name,
                                                   ChannelIdval = Encryption.EncryptID(_channel.ChannelId.ToString(), LoginUserId.ToString()),
                                                   ChannelName= _channel.ChannelName,
                                                   ChannelDescription = _channel.StatusDescription
                                               }).ToListAsync();

            responseData.StatusCode = 1;
            responseData.Message = "Get Success";
            responseData.Data = ChannelsWithOwnerName;
            return responseData;
        }
        catch (ValidationException vex)
        {
            responseData.StatusCode = 0;
            responseData.Message = vex.ValidationResult.ErrorMessage;
            return responseData;
        }
        catch (Exception ex)
        {
            responseData.StatusCode = 0;
            responseData.Message = ex.Message;
            return responseData;
        }
    }

    public async Task<ResponseData> GetChannelProfile(int ChannelId,string destDir)
    {
        ResponseData responseData = new ResponseData();
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
                responseData.Data = "";
            }
            else
            {
                string profileimg = Path.Combine(destDir, profiles.Url);
                if (!System.IO.File.Exists(profileimg))
                {
                    responseData.Data = "";
                }
                else
                {
                    byte[] imageBytes = System.IO.File.ReadAllBytes(profileimg);
                    string base64String = Convert.ToBase64String(imageBytes);
                    responseData.Data =  base64String;
                }
            }
            responseData.StatusCode = 1;
            responseData.Message = "Get Success";
            return responseData;
        }
        catch (ValidationException vex)
        {
            responseData.StatusCode = 0;
            responseData.Message = vex.ValidationResult.ErrorMessage;
            return responseData;
        }
        catch (Exception ex)
        {
            responseData.StatusCode = 0;
            responseData.Message = ex.Message;
            return responseData;
        }
    }

    public async Task<ResponseData> UploadProfile(int LoginUserId, int ChannelId,string filename,string? description)
    {
        ResponseData responseData = new ResponseData();

        try
        {
            var channel = await _db.Channels.Where(x => x.ChannelId == ChannelId)
                                            .FirstOrDefaultAsync();

            if (channel is null) throw new ValidationException("Channel Not Found");
            ViaChannelProfile viaChannelProfile = new ViaChannelProfile();
            viaChannelProfile.ChannelId = ChannelId;
            viaChannelProfile.Url = filename;
            viaChannelProfile.UrlDescription = description;
            await _db.ChannelProfiles.AddAsync(viaChannelProfile.ChangeChannelProfile());
            await _db.SaveChangesAsync();
            responseData.StatusCode = 1;
            responseData.Message = "Save Success";
            return responseData;

        }
        catch (ValidationException vex)
        {
            responseData.StatusCode = 0;
            responseData.Message = vex.ValidationResult.ErrorMessage;
            return responseData;
        }
        catch (Exception ex)
        {
            responseData.StatusCode = 0;
            responseData.Message = ex.Message;
            return responseData;
        }
    }

    public async Task<ResponseData> GenerateChannelUrl(int ChannelId,int LoginUserId)
    {
        ResponseData responseData = new ResponseData();
        try
        {
            string domainUrl = _configuration["appSettings:DomainUrl"] ?? throw new Exception("Invalid DomainUrl");
            string urlSalt = _configuration["appSettings:UrlSalt"] ?? throw new Exception("Invalid UrlSalt");

            var resda = await _db.Channels.Where(x => x.ChannelId == ChannelId).FirstOrDefaultAsync();
            if (resda is null) throw new ValidationException("Channel Not Found");

            string channeldata = $"{LoginUserId}/{ChannelId}";
            string encryptdata = Encryption.EncryptID(channeldata, urlSalt);
            string url = domainUrl +"Channel/" + encryptdata;
            responseData.StatusCode = 1;
            responseData.Message = "Success";
            responseData.Data = url;
            return responseData;
            //responseData.Data = 

        }
        catch (ValidationException vex)
        {
            responseData.StatusCode = 0;
            responseData.Message = vex.ValidationResult.ErrorMessage;
            return responseData;
        }
        catch (Exception ex)
        {
            responseData.StatusCode = 0;
            responseData.Message = ex.Message;
            return responseData;
        }
    }

    public async Task<ResponseData> VisitChannelByInviteLink(string inviteLink,int LoginUserId)
    {
        ResponseData responseData = new ResponseData();
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

                if (reData is null) throw new ValidationException("Channel Not Found");

                VisitChannelHistory inviteHist = new VisitChannelHistory
                {
                    UserId = LoginUserId,
                    InviterId = inviterId,
                    ChannelId = channelId,
                    ViewedDate = DateTime.Now
                };
                await _db.VisitChannelHistories.AddAsync(inviteHist);
                await _db.SaveChangesAsync();
                responseData.StatusCode = 1;
                responseData.Message = "Success";
                responseData.Data = reData;
                return responseData;
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

                if (reData is null) throw new ValidationException("Channel Not Found");
                var statusdata = await (from cm in _db.ChannelMemberships
                                        join st in _db.StatusTypes on cm.StatusId equals st.StatusId
                                        where cm.ChannelId == channelId && cm.UserId == LoginUserId
                                        select new
                                        {
                                            StatusId = st.StatusId,
                                            StatusName = st.StatusName,
                                        }).FirstOrDefaultAsync();
                if (statusdata is null) throw new ValidationException("Member Status Not Found");
                reData.IsMember = statusdata.StatusName.ToLower() == "approved";
                reData.MemberStatus = statusdata.StatusName;
                responseData.StatusCode = 1;
                responseData.Message = "Success";
                responseData.Data = reData;
                return responseData;
            }

        }
        catch (ValidationException vex)
        {
            responseData.StatusCode = 0;
            responseData.Message = vex.ValidationResult.ErrorMessage;
            return responseData;
        }
        catch (Exception ex)
        {
            responseData.StatusCode = 0;
            responseData.Message = ex.Message;
            return responseData;
        }
    }
}
