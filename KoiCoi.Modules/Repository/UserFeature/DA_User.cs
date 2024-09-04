
using KoiCoi.Models.Via;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace KoiCoi.Modules.Repository.UserFeature;

public class DA_User
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _configuration;
    private readonly KcAwsS3Service _awsS3Service;

    public DA_User(AppDbContext db, IConfiguration configuration,KcAwsS3Service awsS3Service)
    {
        _db = db;
        _configuration = configuration;
        _awsS3Service = awsS3Service;
    }

    public async Task<Result<ResponseUserDto>> CreateAccount(ViaUser viaUser,string temppassword)
    {
        Result<ResponseUserDto> model = null;
        try
        {
            if (!(await checkEmailUnique(viaUser.Email ?? "")))
                return Result<ResponseUserDto>.Error("You Email Have Been Registered");
            await _db.Users.AddAsync(viaUser.ChangeUser());
            int result = await _db.SaveChangesAsync();
            if (result == 0)
                return Result<ResponseUserDto>.Error("Registration Fail");
            ResponseUserDto? userData = await _db.Users.Where(x => x.Name == viaUser.Name && x.Password == viaUser.Password)
                .Select(x => new ResponseUserDto
                {
                    UserIdval = x.UserIdval,
                    Name = x.Name
                }).FirstOrDefaultAsync();
            if (userData == null)
                return Result<ResponseUserDto>.Error("Registration Fail");
            string aseKey = _configuration.GetSection("AesEncryption:AseKey").Get<string>()!;
            string aseIv = _configuration.GetSection("AesEncryption:AseIV").Get<string>()!;
            model = Result<ResponseUserDto>.Success(new ResponseUserDto
            {
                UserIdval = userData.UserIdval!,
                Name = userData.Name!,
                Password = AesEncryption.Encrypt(temppassword,aseKey,aseIv)
            });
        }
        catch (Exception ex)
        {
            model = Result<ResponseUserDto>.Error(ex);
        }
        return model;
    }
    private async Task<bool> checkEmailUnique(string email)
    {
        bool unique = false;
        var resultAdmin = await _db.Users.Where(x => x.Email == email).FirstOrDefaultAsync();
        if (resultAdmin == null ||  string.IsNullOrEmpty(email))
        {
            unique = true;///There is no account by this email.
        }
        return unique;
    }

    public async Task<Result<List<UserLoginAccounts>>> AccountsWithDeviceId(string deviceId)
    {
        Result<List<UserLoginAccounts>> result = null;
        try
        {
            byte[] data = Convert.FromBase64String(deviceId);
            string decodeddeviceId = Encoding.UTF8.GetString(data);

            var query = await (from _ah in _db.AccountLoginHistories
                                                   join _user in _db.Users on _ah.UserId equals _user.UserId
                                                   where _ah.DeviceId == decodeddeviceId
                                                   select new //UserLoginAccounts
                                                   {
                                                       UserId = _ah.UserId,
                                                       UserIdval = _user.UserIdval,
                                                       UserName = _user.Name,
                                                       Contact = _user.Email ?? _user.Phone ?? ""
                                                   }).ToListAsync();
            List<UserLoginAccounts> loginUserList = new List<UserLoginAccounts>();
            foreach (var item in query)
            {
                string? img = await _db.UserProfiles.Where(x => x.UserId == item.UserId)
                    .OrderByDescending(up => up.CreatedDate)
                    .Select(x=> x.Url)
                    .FirstOrDefaultAsync();
                UserLoginAccounts newuser = new UserLoginAccounts
                {
                    UserIdval = item.UserIdval,
                    UserName = item.UserName,
                    Contact = item.Contact,
                    UserImage = img ?? ""
                };
                loginUserList.Add(newuser);
            }
            result = Result<List<UserLoginAccounts>>.Success(loginUserList);

        }
        catch (Exception ex)
        {
            result = Result<List<UserLoginAccounts>>.Error(ex);
        }
        return result;
    }

    public async Task<Result<string>> UpdateUserInfo(RequestUserDto requestUserDto,int LoginUserId)
    {
        Result<string> model = null;
        try
        {
            var useData = await _db.Users
                                        .Where(x => x.UserId == LoginUserId).FirstOrDefaultAsync();
            if (useData == null)
                return Result<string>.Error("User Not Found");
            useData.Name = requestUserDto.Name ?? useData.Name!;
            useData.Email = requestUserDto.Email ?? useData.Email!;
            useData.Phone = requestUserDto.Phone ?? useData.Phone;
            useData.DeviceId = requestUserDto.DeviceId ?? useData.DeviceId;
            useData.ModifiedDate = DateTime.UtcNow;
            int result = await _db.SaveChangesAsync();
            if (result == 0)
                return Result<string>.Error("Update Fail");


            model = Result<string>.Success("Update Success");
        }
        catch (Exception ex) 
        {
            model = Result<string>.Error(ex);
        }
        return model;
    }

    /*public async Task<Result<UserInfoResponse>> FindUserByIdval(int userId,int LoginUserId)
    {
        Result<UserInfoResponse> model = null;
        try
        {
            var userData = await _db.Users.Where(x=> x.UserId == userId && x.Inactive == false)
                                        .Select(x=> new UserInfoResponse
                                        {
                                            UserIdval = Encryption.EncryptID(x.UserId.ToString(), LoginUserId.ToString()),
                                            UserName = x.Name
                                        })
                                        .FirstOrDefaultAsync();
            if (userData == null)
                throw new ValidationException("Login User  not found.");

            responseData.StatusCode = 1;
            responseData.Data = userData;
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
     */

    public async Task<Result<List<UserInfoResponse>>> FindUserByName(string name,int LoginUserId)
    {
        Result<List<UserInfoResponse>> model = null;
        try
        {
            var user = await _db.Users.Where(x => x.Name.Contains(name) && x.Inactive == false)
                                           .Select(x=> new 
                                           {
                                               UserId = x.UserId,
                                               UserIdval = Encryption.EncryptID(x.UserId.ToString(), LoginUserId.ToString()),
                                               UserName = x.Name,
                                           })
                                          .ToListAsync();
            List < UserInfoResponse > info = new List < UserInfoResponse >();
            foreach (var u in user)
            {
                var img = await _db.UserProfiles.Where(x => x.UserId == u.UserId)
                    .Select(x=> x.Url)
                    .LastOrDefaultAsync();
                UserInfoResponse newinfo = new UserInfoResponse
                {
                    UserIdval = u.UserIdval,
                    UserName = u.UserName,
                    imgname = img
                };
                info.Add(newinfo);

            }

            model = Result<List<UserInfoResponse>>.Success(info);
        }
        catch (Exception ex)
        {
            model = Result<List<UserInfoResponse>>.Error(ex);
        }

        return model;
    }

    public async Task<dynamic> GetStatusType()
    {
        try
        {
            var agent = await _db.StatusTypes
                .Select(x => new StatusTypeDto
                {
                    StatusId = x.StatusId,
                    StatusName = x.StatusName,
                    StatusDescription = x.StatusDescription,    
                })
                .ToListAsync();
            return agent;
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }

    public async Task<Result<string>> DeleteLoginUser(int LoginUserId)
    {
        Result<string> model = null;
        try
        {
            var userData = await _db.Users.Where(x => x.UserId == LoginUserId).FirstOrDefaultAsync();
            if (userData == null)
                return Result<string>.Error("Login User  not found.");

            userData.ModifiedDate = DateTime.UtcNow;
            userData.Inactive = true;
            await _db.SaveChangesAsync();


            model = Result<string>.Success("Login User Delete Success.You can get your account within 30 days.Please remember your password or your email.");
            
        }
        catch (Exception ex)
        {
            model = Result<string>.Error(ex);
        }
        return model;
    }

    public async Task<Result<string>> UploadUserProfile(UploadUserProfileReqeust payload, int LoginUserId)
    {
        Result<string> model = null;
        try
        {
            if (string.IsNullOrEmpty(payload.base64data)) return Result<string>.Error("Image Not Found");

            /*string? folderPath = _configuration["appSettings:UserProfile"];
            if(folderPath is null) return Result<string>.Error("Invalid temp path.");
            string? baseDirectory = _configuration["appSettings:UploadPath"];
            if (baseDirectory is null) return Result<string>.Error("Invalid UploadPath");

            folderPath = baseDirectory + folderPath;//flodrer import
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string filename = Guid.NewGuid().ToString() + ".png";
            string base64Str = payload.base64data!;
            byte[] bytes = Convert.FromBase64String(base64Str!);

            string filePath = Path.Combine(folderPath, filename);
            if (filePath.Contains(".."))
            { //if found .. in the file name or path
                Log.Error("Invalid path " + filePath);
                return Result<string>.Error("Invalid path");
            }
            await System.IO.File.WriteAllBytesAsync(filePath, bytes);
             */
            string bucketname = _configuration.GetSection("Buckets:UserProfile").Get<string>()!;
            string uniquekey = Globalfunction.NewUniqueFileKey(payload.ext);
            string res=await _awsS3Service.CreateFileAsync(payload.base64data, bucketname, uniquekey, payload.ext);
            if(res == "success")
            {
                UserProfile profile = new UserProfile
                {
                    Url = uniquekey,
                    UrlDescription = payload.description,
                    UserId = LoginUserId,
                    CreatedDate = DateTime.UtcNow,
                };

                await _db.UserProfiles.AddAsync(profile);
                await _db.SaveChangesAsync();
                model = Result<string>.Success("Upload Success");
            }
            else
            {
                model = Result<string>.Error("error");
            }

         }
        catch (Exception ex)
        {
            model = Result<string>.Error(ex);
        }
        return model;
    }

    public async Task<Result<List<UserTypeResponse>>> GetUserTypes(int LoginUserId)
    {
        Result<List<UserTypeResponse>> model = null;
        try
        {
            var query = await _db.UserTypes.Select(x => new UserTypeResponse
            {
                UserTypeIdval = Encryption.EncryptID(x.TypeId.ToString(), LoginUserId.ToString()),
                UserTypeName = x.Name
            }).ToListAsync();
            model = Result<List<UserTypeResponse>>.Success(query);
        }
        catch (Exception ex)
        {
            model = Result<List<UserTypeResponse>>.Error(ex);
        }
        return model;
    }

    public async Task<Result<ResponseUserDto>> Signin(LoginPayload paylod)
    {
        Result<ResponseUserDto> result = null;
        try
        {
            var user = await _db.Users.Where(x=> x.Email == paylod.Email).FirstOrDefaultAsync();
            if(user is null)
                return Result<ResponseUserDto>.Error("Email Not Found");

            string oldsalt = user.PasswordHash!;
            string oldhash = user.Password!;
            bool flag = SaltedHash.Verify(oldsalt, oldhash, paylod.Password);

            if (flag == false)
                return Result<ResponseUserDto>.Error("Incorrect Login Password for user account : " + paylod.Email);

            string aseKey = _configuration.GetSection("AesEncryption:AseKey").Get<string>()!;
            string aseIv = _configuration.GetSection("AesEncryption:AseIV").Get<string>()!;
            result = Result<ResponseUserDto>.Success(new ResponseUserDto
            {
                UserIdval = user.UserIdval!,
                Name = user.Name!,
                Password = AesEncryption.Encrypt(user.Password, aseKey, aseIv)
            });

        }
        catch (Exception ex)
        {
            result = Result<ResponseUserDto>.Error(ex);
        }
        return result;
    }
    public async Task<Result<LoginUserInfo>> GetLoginUserInfo(int LoginEmpID)
    {
        Result<LoginUserInfo> result;
        try
        {
            var user = await (from us in _db.Users
                              join up in _db.UserProfiles on us.UserId equals up.UserId into profiles
                              where us.UserId == LoginEmpID
                              select new
                              {
                                  us.Name,
                                  us.Email,
                                  us.Phone,
                                  LatestProfile = profiles.OrderByDescending(p => p.CreatedDate).FirstOrDefault()
                              })
                  .AsNoTracking()
                  .FirstOrDefaultAsync();

            if (user is null)
                return Result<LoginUserInfo>.Error("User Not Found");
            string purl = "";
            if (!string.IsNullOrEmpty(user.LatestProfile?.Url))
            {
                string url = user.LatestProfile.Url!;
                string bucketname = _configuration.GetSection("Buckets:UserProfile").Get<string>()!;
                Result<string> res= await _awsS3Service.GetFile(bucketname, url);
                if (res.IsSuccess)
                {
                    purl = res.Data;
                }
            }

            var loginUserInfo = new LoginUserInfo
            {
                Name = user.Name,
                Email = user.Email,
                Phone = user.Phone,
                UserImage = purl
            };
            result = Result<LoginUserInfo>.Success(loginUserInfo);

        }
        catch (Exception ex)
        {
            result = Result<LoginUserInfo>.Error(ex);
        }
        return result;
    }

}
