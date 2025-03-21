﻿using Amazon;
using KoiCoi.Models.EventDto.Payload;
using KoiCoi.Models.Via;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace KoiCoi.Modules.Repository.ChannelFeature;

public class DA_Channel
{
    private readonly AppDbContext _db;
    private readonly NotificationManager.NotificationManager _saveNotifications;
    private readonly IConfiguration _configuration;
    private readonly KcAwsS3Service _kcAwsS3Service;

    public DA_Channel(AppDbContext db, 
        IConfiguration configuration, 
        NotificationManager.NotificationManager saveNotifications,
        KcAwsS3Service kcAwsS3Service)
    {
        _db = db;
        _configuration = configuration;
        _saveNotifications = saveNotifications;
        _kcAwsS3Service = kcAwsS3Service;
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

    public async Task<Result<string>> CreateCustomMark(CreateCustomMarkPayload payload, int LoginUserID)
    {
        Result<string> result = null;
        try
        {
            if (string.IsNullOrEmpty(payload.ChannelIdval))
                return Result<string>.Error("Channel Id Can't Null");
            int ChannelId = Convert.ToInt32(Encryption.DecryptID(payload.ChannelIdval, LoginUserID.ToString()));
            foreach (var item in payload.MarkPayloads)
            {
                int? MarkTypeId = null;
                if (!string.IsNullOrEmpty(item.MarkTypeIdval))
                {
                    MarkTypeId = Convert.ToInt32(Encryption.DecryptID(item.MarkTypeIdval, LoginUserID.ToString()));
                }
                int? UserId = null;
                if (!string.IsNullOrEmpty(item.UserIdval))
                {
                    UserId = Convert.ToInt32(Encryption.DecryptID(item.UserIdval, LoginUserID.ToString()));
                    var user = await _db.Users.Where(x => x.UserId == UserId).FirstOrDefaultAsync();
                    if(user is null)
                        return Result<string>.Error("User Id Can't Null");
                }
                Mark newMark = new Mark
                {
                    MarkName = item.MarkName,
                    MarkSymbol = item.MarkSymbol,
                    Isocode = item.Isocode,
                    MarkTypeId = MarkTypeId,
                    ChannelId = ChannelId,
                    UserId = UserId,
                };
                await _db.Marks.AddAsync(newMark);
                await _db.SaveChangesAsync();
            }
            result = Result<string>.Success("Create Success");
        }
        catch (Exception ex)
        {
            result = Result<string>.Error(ex);
        }

        return result;
    }

    public async Task<Result<Pagination>> GetMarkList(int LoginUserId, GetMarkPayload payload)
    {
        Result<Pagination> model = null;
        try
        {
            int pageNumber = payload.pageNumber;
            int pageSize = payload.pageSize;
            int ChannelId = Convert.ToInt32(Encryption.DecryptID(payload.ChannelIdval.ToString(), LoginUserId.ToString()));

            if (string.IsNullOrEmpty(payload.MarkTypeIdval))
            {

                List<MarkResponseDto> newlist = await (from _m in _db.Marks
                                                       join _t in _db.MarkTypes on _m.MarkTypeId equals _t.MarkTypeId
                                                       where  _m.ChannelId == ChannelId
                                                       select new MarkResponseDto
                                                       {
                                                           MarkIdval = Encryption.EncryptID(_m.MarkId.ToString(), LoginUserId.ToString()),
                                                           MarName = _m.MarkName,
                                                           MarkSymbol = _m.MarkSymbol,
                                                           IsoCode = _m.Isocode,
                                                           TypeIdval = Encryption.EncryptID(_t.MarkTypeId.ToString(), LoginUserId.ToString()),
                                                           TypeName = _t.TypeName
                                                       }).ToListAsync();
                Pagination data = RepoFunService.getWithPagination(pageNumber, pageSize, newlist);
                model = Result<Pagination>.Success(data);
            }
            else
            if (payload.MarkTypeIdval.ToLower() == "all")
            {
                List<MarkResponseDto> newlist = await (from _m in _db.Marks
                                                       join _t in _db.MarkTypes on _m.MarkTypeId equals _t.MarkTypeId
                                                       where ( _m.ChannelId != null ? _m.ChannelId == ChannelId : true)
                                                       select new MarkResponseDto
                                                       {
                                                           MarkIdval = Encryption.EncryptID(_m.MarkId.ToString(), LoginUserId.ToString()),
                                                           MarName = _m.MarkName,
                                                           MarkSymbol = _m.MarkSymbol,
                                                           IsoCode = _m.Isocode,
                                                           TypeIdval = Encryption.EncryptID(_t.MarkTypeId.ToString(), LoginUserId.ToString()),
                                                           TypeName = _t.TypeName
                                                       }).ToListAsync();
                Pagination data = RepoFunService.getWithPagination(pageNumber, pageSize, newlist);
                model = Result<Pagination>.Success(data);
            }
            else
            {
                int markTypeId = Convert.ToInt32(Encryption.DecryptID(payload.MarkTypeIdval.ToString(), LoginUserId.ToString()));
                List<MarkResponseDto> newlist = await (from _m in _db.Marks
                                                       join _t in _db.MarkTypes on _m.MarkTypeId equals _t.MarkTypeId
                                                       where _m.MarkTypeId == markTypeId && (_m.ChannelId != null ? _m.ChannelId == ChannelId : true)
                                                       select new MarkResponseDto
                                                       {
                                                           MarkIdval = Encryption.EncryptID(_m.MarkId.ToString(), LoginUserId.ToString()),
                                                           MarName = _m.MarkName,
                                                           MarkSymbol = _m.MarkSymbol,
                                                           IsoCode = _m.Isocode,
                                                           TypeIdval = Encryption.EncryptID(_t.MarkTypeId.ToString(), LoginUserId.ToString()),
                                                           TypeName = _t.TypeName
                                                       }).ToListAsync();
                Pagination data = RepoFunService.getWithPagination(pageNumber, pageSize, newlist);
                model = Result<Pagination>.Success(data);
            }
            /*
                List<MarkResponseDto> newlist = await _db.Marks
                    .J
                    .Select(x => new MarkResponseDto
                    {
                        MarkIdval = Encryption.EncryptID(x.MarkId.ToString(), LoginUserId.ToString()),
                        MarName = x.MarkName,
                        MarkSymbol = x.MarkSymbol,
                        IsoCode = x.Isocode,
                        TypeIdval = Encryption.EncryptID(item.MarkTypeId.ToString(), LoginUserId.ToString()),
                        TypeName = item.TypeName
                    }).ToListAsync();
                marks.Add(newlist);
             * List<List<MarkResponseDto>> marks = new List<List<MarkResponseDto>>();
            var marttype = await _db.MarkTypes.ToListAsync();
            foreach (var item in marttype)
            {
                List<MarkResponseDto> newlist = await _db.Marks.
                    Where(x => x.MarkTypeId == item.MarkTypeId)
                    .Select(x => new MarkResponseDto
                    {
                        MarkIdval = Encryption.EncryptID(x.MarkId.ToString(), LoginUserId.ToString()),
                        MarName = x.MarkName,
                        MarkSymbol = x.MarkSymbol,
                        IsoCode = x.Isocode,
                        TypeIdval = Encryption.EncryptID(item.MarkTypeId.ToString(),LoginUserId.ToString()),
                        TypeName = item.TypeName
                    }).ToListAsync();
                marks.Add(newlist);
            }

            model = Result<Pagination>.Success(marks); //Result<List<List<MarkResponseDto>>>
             */
        }
        catch (Exception ex)
        {
            model = Result<Pagination>.Error(ex);
        }
        return model;
    }

    public async Task<Result<Pagination>> GetMarkType(int LoginUserId, int pageNumber, int pageSize)
    {
        Result<Pagination> resutl = null;
        try
        {
            var types = await _db.MarkTypes
                .Select(x => new
                {
                    TypeId = Encryption.EncryptID(x.MarkTypeId.ToString(),LoginUserId.ToString()),
                    TypeName = x.TypeName
                }).ToListAsync();
            Pagination data = RepoFunService.getWithPagination(pageNumber, pageSize, types);
            resutl = Result<Pagination>.Success(data);
        }
        catch (Exception ex)
        {
            resutl = Result<Pagination>.Error(ex);
        }
        return resutl;
    }

    public async Task<Result<string>> CreateChannel(CreateChannelReqeust channelReqeust, int LoginUserId)
    {
        Result<string> model = null;
        try
        {
            string balanceSalt = _configuration["appSettings:BalanceSalt"] ?? throw new Exception("Invalid Balance Salt");
            int ChanneltypeId = Convert.ToInt32(
                                Encryption.DecryptID(channelReqeust.ChannelTypeval!,
                                LoginUserId.ToString()));
            Channel newchannel = new Channel
            {
                ChannelName = channelReqeust.ChannelName,
                StatusDescription = channelReqeust.StatusDescription,
                ChannelType = ChanneltypeId,
                CreatorId = LoginUserId,
                MemberCount = 1,
                DateCreated = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow,
                Inactive = false
            };
            var addedChannel = await _db.Channels.AddAsync(newchannel);
            int result = await _db.SaveChangesAsync();
            if (result < 1) return Result<string>.Error("Create Channel Fail");

            ///Create MemberShip
            int? ownerid = await _db.UserTypes
                   .Where(x => x.Name == "owner")
                   .Select(x => x.TypeId)
                   .FirstOrDefaultAsync();
            int? approvedstatusId = await _db.StatusTypes
                    .Where(x=> x.StatusName == "Approved")
                    .Select(x=> x.StatusId) .FirstOrDefaultAsync();
            if (ownerid == null) return Result<string>.Error("Owner UserType Not Found");

            if (approvedstatusId == null) return Result<string>.Error("Approved Staus Not Found");
            
            ViaChannelMemberShip newViaChanMeShip = new ViaChannelMemberShip
            {
                ChannelId = addedChannel.Entity.ChannelId,
                UserId = LoginUserId,
                UserTypeId = ownerid.Value,
                StatusId = approvedstatusId.Value
            };
            await _db.ChannelMemberships.AddAsync(newViaChanMeShip.ChangeChannMemberShip());
            await _db.SaveChangesAsync();

            /*
            string filename = "";
            if (!string.IsNullOrEmpty(channelReqeust.ProImage64))
            {
                string baseDirectory = _configuration["appSettings:UploadPath"] ?? throw new Exception("Invalid UploadPath");
                //string tempfolderPath = _configuration["appSettings:UploadChannelProfilePath"] ?? throw new Exception("Invalid temp path.");
                string uploadDirectory = _configuration["appSettings:ChannelProfile"] ?? throw new Exception("Invalid function upload path.");

                string folderPath = Path.Combine(baseDirectory, uploadDirectory);


                filename = Globalfunction.NewUniqueFileName() + ".png";
                string base64Str = channelReqeust.ProImage64;
                byte[] bytes = Convert.FromBase64String(base64Str!);

                string filePath = Path.Combine(folderPath, filename);
                if (filePath.Contains(".."))
                { //if found .. in the file name or path
                    Log.Error("Invalid path " + filePath);
                    throw new Exception("Invalid path");
                }
                await System.IO.File.WriteAllBytesAsync(filePath, bytes);
            }
             */
            /*
             if (!string.IsNullOrEmpty(channelReqeust.ProImage64))
             {
                 string bucketname = _configuration.GetSection("Buckets:ChannelProfile").Get<string>()!;
                 string uniquekey = Globalfunction.NewUniqueFileKey(channelReqeust.ImageExt!);
                 string res = await _kcAwsS3Service.CreateFileAsync(channelReqeust.ProImage64, bucketname, uniquekey, channelReqeust.ImageExt!);
                 ///Save Profile Image
                 ChannelProfile newchPro = new ChannelProfile
                 {
                     Url = uniquekey,
                     UrlDescription = channelReqeust.imagedescription,
                     ChannelId = addedChannel.Entity.ChannelId,
                     CreatedDate = DateTime.UtcNow
                 };

                 await _db.ChannelProfiles.AddAsync(newchPro);
                 await _db.SaveChangesAsync();
             }*/

            ///create channel topic
            var channalTopic = new ChannelTopic
            {
                TopicName = $"{DateTime.UtcNow:yyyyMMddhhmmssssss}",
                Descriptions = $"Topic of {addedChannel.Entity.ChannelName} Channel",
                ChannelId = addedChannel.Entity.ChannelId,
                DateCreated = DateTime.UtcNow
            };

            await _db.ChannelTopics.AddAsync(channalTopic);
            await _db.SaveChangesAsync();

            /*ChannelDataResponse? data = await (from _cha in _db.Channels
                                               join ct in _db.ChannelTypes on _cha.ChannelType equals ct.ChannelTypeId
                                               join cur in _db.Marks on _cha.MarkId equals cur.MarkId
                                               where _cha.ChannelId == addedChannel.Entity.ChannelId
                                               select new ChannelDataResponse
                                               {
                                                   ChannelIdval = Encryption.EncryptID(_cha.ChannelId.ToString(), LoginUserId.ToString()),
                                                   ChannelName = _cha.ChannelName,
                                                   ChannelDescription = _cha.StatusDescription,
                                                   ChannelType = ct.ChannelTypeName,
                                                   ISOCode = cur.Isocode,
                                                   MemberCount = _cha.MemberCount,
                                                   TotalBalance = 0,
                                                   LastBalance = 0,
                                               }).FirstOrDefaultAsync();
            if (data is null) return Result<ChannelDataResponse>.Error("Channel Not Found");
             */
            string channelIdval = Encryption.DecryptID(newchannel.ChannelId.ToString(), LoginUserId.ToString());
            return Result<string>.Success(channelIdval);
        }
        catch (Exception ex)
        {
            return Result<string>.Error(ex);
        }
    }

    public async Task<Result<Pagination>> GetChannelsList(int LoginUserId,int PageNumber,int PageSize, string Status)
    {
        Result<Pagination> model = null;
        try
        {
            int? StatusType = await _db.StatusTypes.Where(x=> x.StatusName.ToLower() == Status.ToLower())
                            .Select(x=> x.StatusId)
                            .FirstOrDefaultAsync();
            if (StatusType == null) return Result<Pagination>.Error("Status Type Not Found");
            string balanceSalt = _configuration["appSettings:BalanceSalt"] ?? throw new Exception("Invalid Balance Salt");
            var  query = await ( from _channel in _db.Channels 
                                               join _chantype in _db.ChannelTypes on _channel.ChannelType equals _chantype.ChannelTypeId
                                               join _mem in _db.ChannelMemberships on _channel.ChannelId equals _mem.ChannelId
                                               join cp in _db.ChannelProfiles on _channel.ChannelId equals cp.ChannelId into chanPro
                                               from cp in chanPro.OrderByDescending(p => p.CreatedDate).Take(1).DefaultIfEmpty()
                                               where _mem.UserId == LoginUserId && _mem.StatusId == StatusType
                                                orderby _channel.ChannelName
                                               select new 
                                               {
                                                   ChannelId=_channel.ChannelId,
                                                   //ChannelIdval = Encryption.EncryptID(_channel.ChannelId.ToString(), LoginUserId.ToString()),
                                                   ChannelName = _channel.ChannelName,
                                                   ChannelDescription = _channel.StatusDescription,
                                                   ChannelType = _chantype.ChannelTypeName,
                                                   MemberCount = _channel.MemberCount,
                                                   CanEdit = _db.UserTypes
                                                            .Where(x => x.TypeId == _mem.UserTypeId)
                                                            .Select(x => x.Name.ToLower())
                                                            .FirstOrDefault() == "owner",
                                                   ChannelProfile = cp != null ? cp.Url : null
                                               }).ToListAsync();
            List<ChannelDataResponse> Channels = new List<ChannelDataResponse> { };
            foreach (var item in query)
            {
                List<ChannelBalanceData> bala = await (from _chbal in _db.ChannelMarkBalances
                                                       join _mk in _db.Marks on _chbal.MarkId equals _mk.MarkId
                                                       where _chbal.ChannelId == item.ChannelId
                                                       select new ChannelBalanceData
                                                       {
                                                           MarkIdval = Encryption.EncryptID(_mk.MarkId.ToString(), LoginUserId.ToString()),
                                                           MarkName = _mk.MarkName,
                                                           IsoCode = _mk.Isocode,
                                                           TotalBalance = Globalfunction.StringToDecimal(Encryption.DecryptID(_chbal.TotalBalance, balanceSalt)),
                                                           LastBalance = Globalfunction.StringToDecimal(Encryption.DecryptID(_chbal.LastBalance, balanceSalt)),
                                                        }).ToListAsync();
                ChannelDataResponse newChannel = new ChannelDataResponse
                {
                    ChannelIdval = Encryption.EncryptID(item.ChannelId.ToString(), LoginUserId.ToString()),
                    ChannelName = item.ChannelName,
                    ChannelDescription = item.ChannelDescription,
                    ChannelType = item.ChannelType,
                    MemberCount = item.MemberCount,
                    CanEdit= item.CanEdit,
                    ChannelProfile = item.ChannelProfile,
                    BalanceDatas = bala
                };
                Channels.Add(newChannel);
            }
            Pagination data = RepoFunService.getWithPagination(PageNumber, PageSize, Channels);
            model = Result<Pagination>.Success(data);
        }
        catch (Exception ex)
        {
            model = Result<Pagination>.Error(ex);
        }
        return model;
    }
    /*
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
    }*/

    /*
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
    }*/


    public async Task<Result<string>> UploadChannelProfile(IFormFile file, string ChannelIdval, int LoginUserId)
    {
        Result<string> result = null;
        try
        {
            int ChannelId = Convert.ToInt32(Encryption.DecryptID(ChannelIdval, LoginUserId.ToString()));
            var resda = await _db.Channels.Where(x => x.ChannelId == ChannelId).FirstOrDefaultAsync();
            if (resda is null) return Result<string>.Error("Channel Not Found");
            string bucketname = _configuration.GetSection("Buckets:ChannelProfile").Get<string>()!;

            string ext = Path.GetExtension(file.FileName);
            string uniquekey = Globalfunction.NewUniqueFileKey(ext);
            Result<string> res = await _kcAwsS3Service.CreateFileAsync(file, bucketname, uniquekey, ext);
            if (res.IsSuccess)
            {
                ChannelProfile channelProfile = new ChannelProfile
                {
                    Url = uniquekey,
                    UrlDescription = "",
                    ChannelId = ChannelId,
                    CreatedDate = DateTime.UtcNow,
                };
                await _db.ChannelProfiles.AddAsync(channelProfile);
                await _db.SaveChangesAsync();
                result = Result<string>.Success("Upload Success");
            }
            else
            {
                result = res;
            }
        }
        catch (Exception ex)
        {
            result = Result<string>.Error(ex);
        }
        return result;
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
            string url = domainUrl +"c/" + encryptdata;
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
            if (!desdata.Contains("/"))
            {
                return Result<VisitChannelResponse>.Error("Incorrect");
            }
            string[] splidata = desdata.Split('/');
            int inviterId = Convert.ToInt32(splidata[0]);
            int channelId = Convert.ToInt32(splidata[1]);
            var IsMember = await _db.ChannelMemberships
                                    .Where(x => x.UserId == LoginUserId && x.ChannelId == channelId)
                                    .FirstOrDefaultAsync();
            if(IsMember is null)
            {

                var reData = await (from ch in _db.Channels
                                                      join ct in _db.ChannelTypes on ch.ChannelType equals ct.ChannelTypeId
                                                      join user in _db.Users on ch.CreatorId equals user.UserId
                                                      join cp in _db.ChannelProfiles on ch.ChannelId equals cp.ChannelId into chanPro
                                                      from cp in chanPro.OrderByDescending(p => p.CreatedDate).Take(1).DefaultIfEmpty()
                                                      where ch.ChannelId == channelId
                                                      select new 
                                                      {
                                                          ChannelId = ch.ChannelId,
                                                          ChannelName = ch.ChannelName,
                                                          ChannelDescription = ch.StatusDescription,
                                                          ChannelType = ct.ChannelTypeName,
                                                          CreatorIdval = Encryption.EncryptID(user.UserId.ToString(), LoginUserId.ToString()),
                                                          CreatorName = user.Name,
                                                          MemberCount = ch.MemberCount,
                                                          ChannelProfile = cp.Url

                                                      }).FirstOrDefaultAsync();

                if (reData is null) return Result<VisitChannelResponse>.Error("Channel Not Found");
                List<ChannelBalanceData> bala = await (from _chbal in _db.ChannelMarkBalances
                                                       join _mk in _db.Marks on _chbal.MarkId equals _mk.MarkId
                                                       where _chbal.ChannelId == reData.ChannelId
                                                       select new ChannelBalanceData
                                                       {
                                                           MarkIdval = Encryption.EncryptID(_mk.MarkId.ToString(), LoginUserId.ToString()),
                                                           MarkName = _mk.MarkName,
                                                           IsoCode = _mk.Isocode,
                                                           TotalBalance = Globalfunction.StringToDecimal(Encryption.DecryptID(_chbal.TotalBalance, balanceSalt)),
                                                           LastBalance = Globalfunction.StringToDecimal(Encryption.DecryptID(_chbal.LastBalance, balanceSalt)),
                                                       }).ToListAsync();
                VisitChannelResponse newChannel = new VisitChannelResponse
                {
                    ChannelIdval = Encryption.EncryptID(reData.ChannelId.ToString(), LoginUserId.ToString()),
                    ChannelName = reData.ChannelName,
                    ChannelDescription = reData.ChannelDescription,
                    ChannelType = reData.ChannelType,
                    MemberCount = reData.MemberCount,
                    ChannelProfile = reData.ChannelProfile,
                    CreatorName = reData.CreatorName,
                    CreatorIdval = reData.CreatorIdval,
                    MemberStatus = null,
                    IsMember = false,
                    BalanceDatas = bala
                };

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
                        ViewedDate = DateTime.UtcNow
                    };
                    await _db.VisitChannelHistories.AddAsync(inviteHist);
                }
                else
                {
                    visitRecord.ViewedDate = DateTime.UtcNow;
                    _db.VisitChannelHistories.Update(visitRecord);
                }
                await _db.SaveChangesAsync();
                model = Result<VisitChannelResponse>.Success(newChannel);
            }
            else
            {

                var reData = await (from ch in _db.Channels
                                    join ct in _db.ChannelTypes on ch.ChannelType equals ct.ChannelTypeId
                                    join user in _db.Users on ch.CreatorId equals user.UserId
                                    join cp in _db.ChannelProfiles on ch.ChannelId equals cp.ChannelId into chanPro
                                    from cp in chanPro.OrderByDescending(p => p.CreatedDate).Take(1).DefaultIfEmpty()
                                    where ch.ChannelId == channelId
                                    select new 
                                    {
                                        ChannelId = ch.ChannelId,
                                        ChannelName = ch.ChannelName,
                                        ChannelDescription = ch.StatusDescription,
                                        ChannelType = ct.ChannelTypeName,
                                        CreatorIdval = Encryption.EncryptID(user.UserId.ToString(), LoginUserId.ToString()),
                                        CreatorName = user.Name,
                                        MemberCount = ch.MemberCount,
                                        ChannelProfile = cp.Url
                                        
                                    }).FirstOrDefaultAsync();

                if (reData is null) return Result<VisitChannelResponse>.Error("Channel Not Found");
                List<ChannelBalanceData> bala = await (from _chbal in _db.ChannelMarkBalances
                                                       join _mk in _db.Marks on _chbal.MarkId equals _mk.MarkId
                                                       where _chbal.ChannelId == reData.ChannelId
                                                       select new ChannelBalanceData
                                                       {
                                                           MarkIdval = Encryption.EncryptID(_mk.MarkId.ToString(), LoginUserId.ToString()),
                                                           MarkName = _mk.MarkName,
                                                           IsoCode = _mk.Isocode,
                                                           TotalBalance = Globalfunction.StringToDecimal(Encryption.DecryptID(_chbal.TotalBalance, balanceSalt)),
                                                           LastBalance = Globalfunction.StringToDecimal(Encryption.DecryptID(_chbal.LastBalance, balanceSalt)),
                                                       }).ToListAsync();
                VisitChannelResponse newChannel = new VisitChannelResponse
                {
                    ChannelIdval = Encryption.EncryptID(reData.ChannelId.ToString(), LoginUserId.ToString()),
                    ChannelName = reData.ChannelName,
                    ChannelDescription = reData.ChannelDescription,
                    ChannelType = reData.ChannelType,
                    MemberCount = reData.MemberCount,
                    ChannelProfile = reData.ChannelProfile,
                    CreatorName = reData.CreatorName,
                    CreatorIdval = reData.CreatorIdval,
                    BalanceDatas = bala
                };
                var statusdata = await (from cm in _db.ChannelMemberships
                                        join st in _db.StatusTypes on cm.StatusId equals st.StatusId
                                        where cm.ChannelId == channelId && cm.UserId == LoginUserId
                                        select new
                                        {
                                            StatusId = st.StatusId,
                                            StatusName = st.StatusName,
                                        }).FirstOrDefaultAsync();
                if (statusdata is null) return Result<VisitChannelResponse>.Error("Member Status Not Found");
                newChannel.IsMember = statusdata.StatusName.ToLower() == "approved";
                newChannel.MemberStatus = statusdata.StatusName;
                model = Result<VisitChannelResponse>.Success(newChannel);
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
            if (!string.IsNullOrEmpty(payload.InviteLink))
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
                        JoinedDate = DateTime.UtcNow,
                        InviterId = inviterId,
                    };
                    await _db.ChannelMemberships.AddAsync(meship);
                    await _db.SaveChangesAsync();
                    model = Result<string>.Success("Joined Success");


                    ///Save to Notification
                    var NotiInfo = await (from chann in _db.ChannelMemberships
                                          join user in _db.Users on chann.UserId equals user.UserId
                                          join inviter in _db.Users on chann.InviterId equals inviter.UserId
                                          join channel in _db.Channels on chann.ChannelId equals channel.ChannelId
                                          where channel.ChannelId == channelId && user.UserId == LoginUserId
                                          select new
                                          {
                                              MembershipId = chann.MembershipId,
                                              UserName = user.Name,
                                              InviterName = user.Name,
                                              ChannelName = channel.ChannelName,
                                              JoinDate = Globalfunction.CalculateDateTime(chann.JoinedDate)
                                          }).FirstOrDefaultAsync();
                    List<int> admins = await (from chan in _db.ChannelMemberships
                                              join admin in _db.Users on chan.UserId equals admin.UserId
                                              join userType in _db.UserTypes on chan.UserTypeId equals userType.TypeId
                                              where chan.ChannelId == channelId &&
                                              (userType.Name.ToLower() == "admin" || userType.Name.ToLower() == "owner")
                                              select admin.UserId).ToListAsync();
                    if (NotiInfo is not null)
                    {
                        await _saveNotifications.SaveNotification(
                            admins,
                            LoginUserId,
                            $"Join New Member to {NotiInfo.ChannelName}",
                            $"{NotiInfo.UserName} who invited by ${NotiInfo.InviterName} Joined the {NotiInfo.ChannelName}",
                            $"JoinedNewMember/{NotiInfo.MembershipId}");
                    }
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
                            model = Result<string>.Warning("You are already member,so you can leave in channel detail");
                        }
                    }
                    else
                    {
                        model = Result<string>.Warning("You are already cancel");
                    }
                }
            }
            else if(!string.IsNullOrEmpty(payload.ChannelIdval))
            {
                int channelId = Convert.ToInt32(Encryption.DecryptID(payload.ChannelIdval, LoginUserId.ToString()));
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
                        model = Result<string>.Warning("You are already member,so you can leave in channel detail");
                    }
                }
                else
                {
                    model = Result<string>.Warning("You are already cancel");
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
            string uploadDirectory = _configuration["appSettings:ChannelProfile"] ?? throw new Exception("Invalid function upload path.");
            string destDirectory = Path.Combine(baseDirectory, uploadDirectory);


            var loginUserType = await (from ch in _db.ChannelMemberships
                                              join ut in _db.UserTypes on ch.UserTypeId equals ut.TypeId
                                              where ch.ChannelId == channelId && ch.UserId == LoginUserId
                                              select new
                                              {
                                                  UserTypeName = ut.Name
                                              }).FirstOrDefaultAsync();
            if(loginUserType is null || (statusType.StatusName.ToLower() == "pending" && loginUserType.UserTypeName.ToLower() == "member") )
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
                                                           JoinedDate = ch.JoinedDate.ToString(),//Globalfunction.CalculateDateTime(ch.JoinedDate) ,
                                                           UserImageUrl = ""
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
                    string profileimg = Path.Combine(destDirectory, userImageUrl );
                    byte[] imageBytes = System.IO.File.ReadAllBytes(profileimg);
                    string base64String = Convert.ToBase64String(imageBytes);
                    response.UserImage64 = base64String;
                }*/
                string? imageUrl = await (from _up in _db.UserProfiles
                                          where _up.UserId == Convert.ToInt32(response.MemberIdval)
                                          orderby _up.CreatedDate descending // Assuming UploadDate is the timestamp of the upload
                                          select _up.Url).FirstOrDefaultAsync();
                response.UserImageUrl = imageUrl;
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
                        var membershi = await _db.ChannelMemberships.Where(x => x.MembershipId == MembershipId)
                                                .FirstOrDefaultAsync();
                        if (membershi is not null && membershi.StatusId != ApproveStatus)
                        {
                            membershi.StatusId = ApproveStatus;
                            membershi.UserTypeId = userTypeId;
                            _db.ChannelMemberships.Update(membershi);
                            await _db.SaveChangesAsync();

                            //update channel member count
                            var channel = await _db.Channels.Where(x => x.ChannelId == membershi.ChannelId)
                                                .FirstOrDefaultAsync();

                            if (channel is not null)
                            {
                                channel.MemberCount = channel.MemberCount + 1;
                                _db.Channels.Update(channel);
                                await _db.SaveChangesAsync();
                            }
                            ///Save Notification
                            ///Welcome New Member
                            List<int> users = await (from meship in _db.ChannelMemberships
                                                  join chan in _db.Channels on meship.ChannelId equals chan.ChannelId
                                                  join status in _db.StatusTypes on meship.StatusId equals status.StatusId
                                                  where meship.ChannelId == membershi.ChannelId
                                                  && status.StatusName.ToLower() == "approved"
                                                  select meship.UserId).ToListAsync();
                            var UserName = await (from me in _db.ChannelMemberships
                                                  join user in _db.Users on me.UserId equals user.UserId
                                                  join inviter in _db.Users on me.InviterId equals inviter.UserId
                                                  join channl in _db.Channels on me.ChannelId equals channl.ChannelId
                                                  where me.MembershipId == membershi.MembershipId
                                                  select new
                                                  {
                                                     UserName = user.Name,
                                                     ChannelName = channl.ChannelName,
                                                     inviterName = inviter.Name,
                                                     JoinedDate = Globalfunction.CalculateDateTime(me.JoinedDate)
                                                  }).FirstOrDefaultAsync();
                            if (users.Contains(LoginUserId))
                            {
                                users.Remove(LoginUserId);
                            }

                            await _saveNotifications.SaveNotification(
                                users,
                                LoginUserId,
                                $"{UserName?.UserName} Joined the channel {UserName?.ChannelName}",
                                $"{UserName?.UserName} who invited by {UserName?.inviterName} Joined the {UserName?.ChannelName}",
                                $"ActionByChannelAdminToJoinedMember/{membershi.MembershipId}"
                                );
                        }

                    }
                    ///2 to reject
                    else if(item.ApproveStatus == 2)
                    {
                        int RejectStatus = await _db.StatusTypes.Where(x => x.StatusName.ToLower() == "rejected")
                            .Select(x => x.StatusId).FirstOrDefaultAsync();
                        var membersh = await _db.ChannelMemberships.Where(x => x.MembershipId == MembershipId)
                                                .FirstOrDefaultAsync();
                        if (membersh is not null)
                        {
                            membersh.StatusId = RejectStatus;
                            _db.ChannelMemberships.Update(membersh);
                            await _db.SaveChangesAsync();
                        }
                    }
                    ///Save Notification
                    ///Inform to User and Admins for Action
                    var membership = await _db.ChannelMemberships.Where(x => x.MembershipId == MembershipId)
                                                .FirstOrDefaultAsync();
                    if(membership is not null)
                    {
                        var NotiInfo = await (from chann in _db.ChannelMemberships
                                              join user in _db.Users on chann.UserId equals user.UserId
                                              join inviter in _db.Users on chann.InviterId equals inviter.UserId
                                              join channel in _db.Channels on chann.ChannelId equals channel.ChannelId
                                              where channel.ChannelId == membership.ChannelId && user.UserId == membership.UserId
                                              select new
                                              {
                                                  MembershipId = chann.MembershipId,
                                                  UserName = user.Name,
                                                  InviterName = user.Name,
                                                  ChannelName = channel.ChannelName,
                                                  JoinDate = Globalfunction.CalculateDateTime(chann.JoinedDate)
                                              }).FirstOrDefaultAsync();
                        List<int> admins = await (from chan in _db.ChannelMemberships
                                                  join admin in _db.Users on chan.UserId equals admin.UserId
                                                  join userType in _db.UserTypes on chan.UserTypeId equals userType.TypeId
                                                  where chan.ChannelId == membership.ChannelId &&
                                                  (userType.Name.ToLower() == "admin" || userType.Name.ToLower() == "owner")
                                                  select admin.UserId).ToListAsync();
                        admins.Add(membership.UserId);
                        if (admins.Contains(LoginUserId))
                        {
                            admins.Remove(LoginUserId);
                        }
                        string? LoginName = await _db.Users.Where(x => x.UserId == LoginUserId)
                            .Select(x => x.Name).FirstOrDefaultAsync();
                        string actionN = "";
                        if(item.ApproveStatus == 1)
                        {
                            actionN = "Approved";
                        }else if(item.ApproveStatus == 2)
                        {
                            actionN = "Rejected";
                        }
                        if (NotiInfo is not null)
                        {
                            await _saveNotifications.SaveNotification(
                                admins,
                                LoginUserId,
                                $"{LoginName} {actionN} to {NotiInfo.UserName} in {NotiInfo.ChannelName}",
                                $"{NotiInfo.UserName} who invited by ${NotiInfo.InviterName} was {actionN} by {LoginName}",
                                $"JoinedNewMember/{NotiInfo.MembershipId}");
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

    public async Task<Result<List<VisitUserResponse>>> GetVisitUsersRecords(GetVisitUsersPayload payload, int LoginUserId)
    {
        Result<List<VisitUserResponse>> model = null;
        try
        {
            int channelId = Convert.ToInt32(Encryption.DecryptID(payload.ChannelIdval!,LoginUserId.ToString()));
            
            DateTime date;
            if(DateTime.TryParseExact(payload.Date!, "yyyy-MM", null, System.Globalization.DateTimeStyles.None, out date))
            {
                List<VisitUserResponse> query = await (from visit in _db.VisitChannelHistories
                                   join chan in _db.Channels on visit.ChannelId equals chan.ChannelId
                                   join visituser in _db.Users on visit.UserId equals visituser.UserId
                                   join inviter in _db.Users on visit.InviterId equals inviter.UserId
                                   join meme in _db.ChannelMemberships on chan.ChannelId equals meme.ChannelId
                                   where visit.ChannelId == channelId
                                   && meme.UserId == LoginUserId && meme.StatusId == 2//Approved Status
                                   && visit.ViewedDate.Year == date.Year 
                                   && visit.ViewedDate.Month == date.Month
                                   orderby visit.ViewedDate descending
                                   select new VisitUserResponse
                                   {
                                       UserIdval = Encryption.EncryptID(visituser.UserId.ToString(), LoginUserId.ToString()),
                                       UserName = visituser.Name,
                                       InviterIdval = Encryption.EncryptID(inviter.UserId.ToString(),LoginUserId.ToString()),
                                       InviterName = inviter.Name,
                                       VisitedDate = Globalfunction.CalculateDateTime(visit.ViewedDate),
                                   }).ToListAsync();
                model = Result<List<VisitUserResponse>>.Success(query);

            }
            else
            {
                model = Result<List<VisitUserResponse>>.Error("Wroung Date Formate");
            }
            
        }
        catch (Exception ex)
        {
            model = Result<List<VisitUserResponse>>.Error(ex);
        }
        return model;
    }

    public async Task<Result<List<VisitUserResponse>>> NewMembersRecords(GetVisitUsersPayload payload, int LoginUserId)
    {
        Result<List<VisitUserResponse>> model = null;
        try
        {
            int channelId = Convert.ToInt32(Encryption.DecryptID(payload.ChannelIdval!, LoginUserId.ToString()));

            DateTime date;
            if (DateTime.TryParseExact(payload.Date!, "yyyy-MM", null, System.Globalization.DateTimeStyles.None, out date))
            {
                List<VisitUserResponse> query = await (from chanmeb in _db.ChannelMemberships
                                                       join chan in _db.Channels on chanmeb.ChannelId equals chan.ChannelId
                                                       join newme in _db.Users on chanmeb.UserId equals newme.UserId
                                                       join inviter in _db.Users on chanmeb.InviterId equals inviter.UserId
                                                       join meme in _db.ChannelMemberships on chan.ChannelId equals meme.ChannelId
                                                       where chanmeb.ChannelId == channelId
                                                       && meme.UserId == LoginUserId && meme.StatusId == 2//Approved Status
                                                       && chanmeb.StatusId == 2
                                                       && chanmeb.JoinedDate.Year == date.Year
                                                       && chanmeb.JoinedDate.Month == date.Month
                                                       orderby chanmeb.JoinedDate descending
                                                       select new VisitUserResponse
                                                       {
                                                           UserIdval = Encryption.EncryptID(newme.UserId.ToString(), LoginUserId.ToString()),
                                                           UserName = newme.Name,
                                                           InviterIdval = Encryption.EncryptID(inviter.UserId.ToString(), LoginUserId.ToString()),
                                                           InviterName = inviter.Name,
                                                           VisitedDate = Globalfunction.CalculateDateTime(chanmeb.JoinedDate),
                                                       }).ToListAsync();
                model = Result<List<VisitUserResponse>>.Success(query);

            }
            else
            {
                model = Result<List<VisitUserResponse>>.Error("Wroung Date Formate");
            }
        }
        catch (Exception ex) { 
            model = Result<List<VisitUserResponse>>.Error(ex);
        }

        return model;
    }

    public async Task<Result<string>> LeaveChannel(string channelIdval, int LoginUserId)
    {
        Result<string> model = null;
        try
        {
            int channelId = Convert.ToInt32(Encryption.DecryptID(channelIdval, LoginUserId.ToString()));
            var membership = await _db.ChannelMemberships
                                    .Where(x => x.ChannelId == channelId && x.UserId == LoginUserId)
                                    .FirstOrDefaultAsync();
            if (membership is null) {
                model = Result<string>.Error("Member Not Found");
            }
            else
            {
                var channel = await _db.Channels.Where(x => x.ChannelId == channelId).FirstOrDefaultAsync();
                if (channel is null) return Result<string>.Error("Channel Not Found");
                channel.MemberCount = channel.MemberCount - 1;
                _db.Channels.Update(channel);
                _db.ChannelMemberships.Remove(membership);
                await _db.SaveChangesAsync();
                model = Result<string>.Success("Success");


                ///Save Notification
                List<int> users = await (from _me in _db.ChannelMemberships
                                   join chan in _db.Channels on _me.ChannelId equals chan.ChannelId
                                   where chan.ChannelId == channel.ChannelId
                                   select _me.UserId).ToListAsync();
                string? Name = await _db.Users.Where(x => x.UserId == LoginUserId).Select(x => x.Name).FirstOrDefaultAsync();
                await _saveNotifications.SaveNotification(users,
                    LoginUserId,
                    $"{Name} leaved from {channel.ChannelName}",
                    $"{Name} leaved from {channel.ChannelName}",
                    $"LeaveChannel/{LoginUserId}");
            }
        }
        catch (Exception ex)
        {
            model = Result<string>.Error(ex);
        }
        return model;
    }

    public async Task<Result<string>> RemoveMemberByAdmin(string channelIdval, List<RemoveMemberData> memberdatas, int LoginUserId)
    {
        Result<string> model = null;
        try
        {
            int channelId = Convert.ToInt32(Encryption.DecryptID(channelIdval, LoginUserId.ToString()));
            var checkLoginUserAccess = await (from meship in _db.ChannelMemberships
                                              join channal in _db.Channels on meship.ChannelId equals channal.ChannelId
                                              join usertype in _db.UserTypes on meship.UserTypeId equals usertype.TypeId
                                              where meship.UserId == LoginUserId && meship.ChannelId == channelId
                                             && (usertype.Name.ToLower() == "admin" || usertype.Name.ToLower() == "owner")
                                             select new
                                             {
                                                 UserType = usertype.Name
                                             }).FirstOrDefaultAsync();
            if (checkLoginUserAccess == null) return Result<string>.Error("Login User Can't access to remove member");
            foreach (var memberdata in memberdatas)
            {
                int memberId = Convert.ToInt32(Encryption.DecryptID(memberdata.MemberIdval!, LoginUserId.ToString()));
                var membership = await _db.ChannelMemberships
                                    .Where(x => x.ChannelId == channelId && x.UserId == memberId)
                                    .FirstOrDefaultAsync();
                if (membership is not null)
                {
                    var channel = await _db.Channels.Where(x => x.ChannelId == channelId).FirstOrDefaultAsync();
                    if (channel is null) return Result<string>.Error("Channel Not Found");
                    channel.MemberCount = channel.MemberCount - 1;
                    _db.Channels.Update(channel);
                    _db.ChannelMemberships.Remove(membership);
                    await _db.SaveChangesAsync();
                    RemoveMemberHistory data = new RemoveMemberHistory
                    {
                        AdminId = LoginUserId,
                        MemberId = memberId,
                        ChannelId = channelId,
                        Reason = memberdata.Reason ?? "",
                        RemoveDate = DateTime.UtcNow
                    };
                    await _db.RemoveMemberHistories.AddAsync(data);
                    await _db.SaveChangesAsync();

                    List<int> users = await _db.ChannelMemberships.Where(x => x.ChannelId == channelId)
                        .Select(x => x.UserId).ToListAsync();
                    string? userName = await _db.Users.Where(x => x.UserId == LoginUserId)
                        .Select(x => x.Name).FirstOrDefaultAsync();
                    string? memberName = await _db.Users.Where(x => x.UserId == memberId)
                        .Select(x => x.Name).FirstOrDefaultAsync();
                    users.Add(membership.UserId);
                    if (users.Contains(LoginUserId))
                    {
                        users.Remove(LoginUserId);
                    }
                    await _saveNotifications.SaveNotification(
                        users,
                        LoginUserId,
                        $"{userName} removed {memberName}",
                        $"{userName} removed {memberName} because {memberdata.Reason ?? ""}",
                        $"RemoveMember/{memberId}"
                        );
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

    public async Task<Result<string>> ChangeUserTypeTheChannelMemberships(ChangeUserTypeChannelMembership payload, int LoginUserId)
    {
        Result<string> result = null;
        try
        {
            int ChannelId = Convert.ToInt32(Encryption.DecryptID(payload.ChannelIdval!, LoginUserId.ToString()));
            List<UserIdAndUserType> userIdAndUserTypes = payload.userIdAndUserTypes!;
            foreach (var item in userIdAndUserTypes)
            {
                int UserId = Convert.ToInt32(Encryption.DecryptID(item.UserIdval!, LoginUserId.ToString()));
                int UserTypeId = Convert.ToInt32(Encryption.DecryptID(item.UserTypeIdval!, LoginUserId.ToString()));
                ChannelMembership? ChanelMembership = await _db.ChannelMemberships
                    .Where(x => x.ChannelId == ChannelId &&
                    x.UserId == UserId).FirstOrDefaultAsync();
                if(ChanelMembership is not null)
                {
                    ChanelMembership.UserTypeId = UserTypeId;
                    await _db.SaveChangesAsync();


                    ///Save Noti to admins
                    var admins = await (from _meme in _db.ChannelMemberships
                                        join _ut in _db.UserTypes on _meme.UserTypeId equals _ut.TypeId
                                        where _meme.ChannelId == ChannelId
                                        && (_ut.Name.ToLower() == "owner" || _ut.Name.ToLower() == "admin")
                                        select _meme.UserId).ToListAsync();
                    admins.Add(UserId);
                    if (admins.Contains(LoginUserId))
                    {
                        admins.Remove(LoginUserId);
                    }
                    var data = await (from _meme in _db.ChannelMemberships
                                      join _user in _db.Users on _meme.UserId equals _user.UserId
                                      join _ut in _db.UserTypes on _meme.UserTypeId equals _ut.TypeId
                                      where _meme.ChannelId == ChannelId
                                      && _meme.UserId == UserId
                                      select new
                                      {
                                          UserName = _user.Name,
                                          UserType = _ut.Name
                                      }).FirstOrDefaultAsync();
                    string? loginname = _db.Users.Where(x => x.UserId == LoginUserId).Select(x => x.Name).FirstOrDefault();
                    if(data is not null && loginname is not null)
                    {
                       await _saveNotifications.SaveNotification(admins,
                            LoginUserId,
                            $"Changed the UserType of {data.UserName}",
                            $"{loginname} Changed {data.UserName} to {data.UserType}",
                            $"ChannelUserTypeChange/{ChanelMembership.MembershipId}");
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

    public async Task<Result<ChannelAccessMenu>> CheckChannelAccessMenu(GetChannelData payload, int LoginUserID)
    {
        Result<ChannelAccessMenu> result = null;
        try
        {
            int ChannelId = Convert.ToInt32(Encryption.DecryptID(payload.channelIdval!, LoginUserID.ToString()));
            ChannelAccessMenu accessMenu = new ChannelAccessMenu {
                CanMemberReview = false,
                CanEventReview = false,
            };

            var admin = await (from _ch in _db.Channels
                               join _cm in _db.ChannelMemberships on _ch.ChannelId equals _cm.ChannelId
                               join _ut in _db.UserTypes on _cm.UserTypeId equals _ut.TypeId
                               where _cm.UserId == LoginUserID && 
                               _ch.ChannelId == ChannelId &&
                               (_ut.Name.ToLower() == "admin" || _ut.Name.ToLower() == "owner")
                               select _cm).FirstOrDefaultAsync();
            if(admin is not null)
            {
                accessMenu.CanMemberReview = true;
                accessMenu.CanEventReview = true;
            }
            result = Result<ChannelAccessMenu>.Success(accessMenu);
        }
        catch (Exception ex)
        {
            result = Result<ChannelAccessMenu>.Error(ex);
        }
        return result;
    }

    public async Task<Result<Pagination>> FindAccessChannelByName(FindByNamePayload payload, int LoginUserID)
    {
        Result<Pagination> result = null;
        try
        {
            string? Name = payload.Name;
            var query = await (from _ch in _db.Channels
                                               join _cm in _db.ChannelMemberships on _ch.ChannelId equals _cm.ChannelId
                                                join _status in _db.StatusTypes on _cm.StatusId equals _status.StatusId
                                                where _cm.UserId == LoginUserID && _status.StatusName.ToLower() == "approved" 
                                               && (string.IsNullOrEmpty(Name) ? true : _ch.ChannelName.Contains(Name))
                                               select new
                                               {
                                                   ChannelIdval = Encryption.EncryptID(_ch.ChannelId.ToString(), LoginUserID.ToString()),
                                                   ChannelName = _ch.ChannelName
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
    public async Task<Result<Pagination>> ChannelOverallContribution(OverallContributionPayload payload, int LoginUserId)
    {
        Result<Pagination> result = null;
        try
        {

            string balanceSalt = _configuration["appSettings:BalanceSalt"] ?? throw new Exception("Invalid Balance Salt");
            int ChannelId = Convert.ToInt32(Encryption.DecryptID(payload.Idval, LoginUserId.ToString()));

            // Fetch allowed marks for the event
            List<Mark> allowedMarks = await (from _mark in _db.Marks
                                             join _cb in _db.ChannelMarkBalances on _mark.MarkId equals _cb.MarkId
                                             where _cb.ChannelId == ChannelId
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
                                        join _pro in _db.UserProfiles on _user.UserId equals _pro.UserId into pro
                                        where _members.ChannelId == ChannelId
                                        select new
                                        {
                                            UserId = _user.UserId,
                                            Name = _user.Name,
                                            Email = _user.Email,
                                            Image = pro.OrderByDescending(p => p.CreatedDate)
                                            .Select(x=> x.Url)
                                            .FirstOrDefault()
                                        })
                                        .Distinct()
                                        .ToListAsync();


            foreach (var member in members)
            {
                List<ContributionResponse> contributions = new List<ContributionResponse>();

                foreach (var mark in allowedMarks)
                {
                    var query = await (from _coll in _db.CollectPosts
                                       join _st in _db.StatusTypes on _coll.StatusId equals _st.StatusId
                                       join _colBal in _db.PostBalances on _coll.PostId equals _colBal.PostId
                                       join _ev in _db.Events on _coll.EventPostId equals _ev.PostId
                                       join _cb in _db.ChannelMarkBalances on _ev.ChannelId equals _cb.ChannelId
                                       where _colBal.MarkId == _cb.MarkId &&
                                             mark.MarkId == _colBal.MarkId &&
                                             mark.MarkId == _cb.MarkId &&
                                             (_st.StatusName.ToLower() == "approved" || _st.StatusName.ToLower() == "pending") &&
                                             _cb.ChannelId == ChannelId &&
                                             _coll.CreatorId == member.UserId
                                       group new
                                       {
                                           _colBal.Balance,
                                           _cb.TotalBalance
                                       }
                                       by _cb.TotalBalance into grouped
                                       select new
                                       {
                                           CollectBalance = grouped.Select(x => x.Balance).ToList(),
                                           TotalBalance = grouped.Key
                                       }).ToListAsync();

                    // If no contributions are found, add a default contribution for the mark
                    if (!query.Any())
                    {
                        string? totalBalance = await _db.ChannelMarkBalances
                                                  .Where(x => x.ChannelId == ChannelId && x.MarkId == mark.MarkId)
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
}
