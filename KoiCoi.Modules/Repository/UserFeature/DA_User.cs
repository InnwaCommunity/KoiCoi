﻿
using KoiCoi.Database.AppDbContextModels;
using KoiCoi.Models.User_Dto.Payload;
using KoiCoi.Models.Via;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using NuGet.Protocol.Core.Types;
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

    public async Task<Result<ResponseUserDto>> CreateAccount(RequestUserDto requestUser)
    {
        Result<ResponseUserDto> model = null;
        try
        {

            string aseKey = _configuration.GetSection("AesEncryption:AseKey").Get<string>()!;
            string aseIv = _configuration.GetSection("AesEncryption:AseIV").Get<string>()!;
            if (requestUser.Password is null)
                return Result<ResponseUserDto>.Error("Invalide Password");

            if (!(await checkEmailUnique(requestUser.Email ?? "")))
                return Result<ResponseUserDto>.Error("Your Email Have Been Registered");
            string payloadpassword = AesEncryption.Decrypt(requestUser.Password, aseKey, aseIv);

            // Generate a new 12-character password with at least 1 non-alphanumeric character.
            RandomPassword passwordGenerator = new RandomPassword();
            string name = "Guest";
            if (!string.IsNullOrEmpty(requestUser.Name))
            {
                name = requestUser.Name;
            }
            string salt = SaltedHash.GenerateSalt();
            string password = SaltedHash.ComputeHash(salt, payloadpassword.ToString());
            string userIdval = Encryption.EncryptID(name, salt) + passwordGenerator.CreatePassword(name.Length, name.Length / 3);

            User user = new User
            {
                UserIdval = userIdval,
                Name = name,
                Email = requestUser.Email,
                Password = password,
                PasswordHash = salt,
                Phone = requestUser.Phone,
                DeviceId = requestUser.DeviceId,
                DateCreated = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow,
                Inactive = false,
            };
            var newUserEntry = await _db.Users.AddAsync(user);
            int result = await _db.SaveChangesAsync();
            if (result == 0 || newUserEntry.Entity == null)
                return Result<ResponseUserDto>.Error("Registration Failed");

            // Prepare the success response model
            return model = Result<ResponseUserDto>.Success(new ResponseUserDto
            {
                UserIdval = newUserEntry.Entity.UserIdval,
                Name = newUserEntry.Entity.Name!,
                Password = AesEncryption.Encrypt(payloadpassword, aseKey, aseIv)
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
            string bucketname = _configuration.GetSection("Buckets:UserProfile").Get<string>()!;
            byte[] data = Convert.FromBase64String(deviceId);
            string decodeddeviceId = Encoding.UTF8.GetString(data);

            var query = await (from _ah in _db.AccountLoginHistories
                               join _user in _db.Users on _ah.UserId equals _user.UserId
                               where _ah.DeviceId == decodeddeviceId
                               orderby _ah.ModifiedData descending
                               select new
                               {
                                   UserId = _ah.UserId,
                                   UserIdval = _user.UserIdval,
                                   UserName = _user.Name,
                                   Contact = _user.Email ?? _user.Phone ?? ""
                               })
                   .ToListAsync();

            List<UserLoginAccounts> loginUserList = new List<UserLoginAccounts>();
            foreach (var item in query)
            {
                string? img = await _db.UserProfiles.Where(x => x.UserId == item.UserId)
                    .OrderByDescending(up => up.CreatedDate)
                    .Select(x=> x.Url)
                    .FirstOrDefaultAsync();
                string presignedUrl = "";
                if (!string.IsNullOrEmpty(img))
                {
                    Result<string> predata=await _awsS3Service.GetFile(bucketname, img);
                    if (predata.IsSuccess)
                    {
                        presignedUrl = predata.Data;
                    }
                }
                UserLoginAccounts newuser = new UserLoginAccounts
                {
                    UserIdval = item.UserIdval,
                    UserName = item.UserName,
                    Contact = item.Contact,
                    UserImage = presignedUrl
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

    public async Task<Result<string>> UploadUserProfile(IFormFile file, int LoginUserId)
    {
        Result<string> model = null;
        try
        {
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
            string ext = Path.GetExtension(file.FileName);
            string uniquekey = Globalfunction.NewUniqueFileKey(ext);
            Result<string> res=await _awsS3Service.CreateFileAsync(file, bucketname, uniquekey, ext);
            if(res.IsSuccess)
            {
                UserProfile profile = new UserProfile
                {
                    Url = uniquekey,
                    UrlDescription = "",
                    UserId = LoginUserId,
                    CreatedDate = DateTime.UtcNow,
                };

                await _db.UserProfiles.AddAsync(profile);
                await _db.SaveChangesAsync();
                model = Result<string>.Success("Upload Success");
            }
            else
            {
                model = res;
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
            var user = await _db.Users.Where(
                x=> x.Email == paylod.Email && x.FacebookUserId==null && x.GoogleUserId==null
                ).FirstOrDefaultAsync();
            if(user is null)
                return Result<ResponseUserDto>.Error("Email Not Found");

            string aseKey = _configuration.GetSection("AesEncryption:AseKey").Get<string>()!;
            string aseIv = _configuration.GetSection("AesEncryption:AseIV").Get<string>()!;
            string realpass = AesEncryption.Decrypt(paylod.Password, aseKey, aseIv);
            string oldsalt = user.PasswordHash!;
            string oldhash = user.Password!;
            bool flag = SaltedHash.Verify(oldsalt, oldhash, realpass);

            if (flag == false)
                return Result<ResponseUserDto>.Error("Incorrect Login Password for user account : " + paylod.Email);

            result = Result<ResponseUserDto>.Success(new ResponseUserDto
            {
                UserIdval = user.UserIdval!,
                Name = user.Name!,
                Password = paylod.Password
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

    public async Task<Result<string>> ChangeLoginPassword(ChangePasswordPayload paylod, int LoginUserId)
    {
        Result<string> result = null;
        try
        {
            var user = await _db.Users.Where(x => x.UserId == LoginUserId).FirstOrDefaultAsync();
            if (user is null)
                return Result<string>.Error("Email Not Found");

            string aseKey = _configuration.GetSection("AesEncryption:AseKey").Get<string>()!;
            string aseIv = _configuration.GetSection("AesEncryption:AseIV").Get<string>()!;
            string oldPassword = AesEncryption.Decrypt(paylod.OldPassword, aseKey, aseIv);
            string newPassword = AesEncryption.Decrypt(paylod.NewPassword, aseKey, aseIv);
            string oldsalt = user.PasswordHash!;
            string oldhash = user.Password!;
            bool flag = SaltedHash.Verify(oldsalt, oldhash, oldPassword);

            if (flag == false)
                return Result<string>.Error("Incorrect Login Password for user account : ");

            string salt = SaltedHash.GenerateSalt();
            user.Password = SaltedHash.ComputeHash(salt, newPassword);
            user.PasswordHash = salt;
            await _db.SaveChangesAsync();
            result = Result<string>.Success("Change Success");
        }
        catch (Exception ex)
        {
            result = Result<string>.Error(ex);
        }
        return result;
    }

    public async Task<Result<string>> RemoveLoginAccount(RemoveLoginAccountPayload payload)
    {
        Result<string> result = null;
        try
        {
            var loginhistory = await (from _user in _db.Users
                                      join _history in _db.AccountLoginHistories on _user.UserId equals _history.UserId
                                      where _user.UserIdval == payload.UserIdval && _history.DeviceId == payload.DeviceId
                                      select _history).FirstOrDefaultAsync();
            if( loginhistory is not null)
            {
                 _db.Remove(loginhistory);
                int success = await _db.SaveChangesAsync();
                if(success > 0)
                {
                    result = Result<string>.Success("Remove Success");
                }
                else
                {
                    result = Result<string>.Error("Fail");
                }
            }
            else
            {
                result = Result<string>.Success("Remove Success");
            }
        }
        catch (Exception ex)
        {
            result = Result<string>.Error(ex);
        }
        return result;
    }


    public async Task<Result<string>> GetUserProfile(GetUserData payload, int LoginEmpID)
    {
        Result<string> result;
        try
        {
            int UserId = LoginEmpID;
            if (!string.IsNullOrEmpty(payload.UserIdval))
            {
                UserId = Convert.ToInt32(Encryption.DecryptID(payload.UserIdval, LoginEmpID.ToString()));
            }
            var LatestProfile = await (from us in _db.Users
                              join up in _db.UserProfiles on us.UserId equals up.UserId into profiles
                              where us.UserId == UserId
                              select new
                              {
                                  Url = profiles.OrderByDescending(p => p.CreatedDate)
                                  .Select(x=> x.Url)
                                  .FirstOrDefault()
                              })
                  .AsNoTracking()
                  .FirstOrDefaultAsync();

            if (LatestProfile is null || LatestProfile.Url is null)
                return Result<string>.Error("Image Not Found");
            string purl = "";
            if (!string.IsNullOrEmpty(LatestProfile.Url))
            {
                string url = LatestProfile.Url!;
                string bucketname = _configuration.GetSection("Buckets:UserProfile").Get<string>()!;
                Result<string> res = await _awsS3Service.GetFile(bucketname, url);
                if (res.IsSuccess)
                {
                    purl = res.Data;
                }
                else
                {
                    return Result<string>.Error("Image Not Found");
                }
            }
            result = Result<string>.Success(purl);
        }
        catch (Exception ex)
        {
            result = Result<string>.Error(ex);
        }
        return result;
    }
    public async Task<Result<UserLoginAccounts>> CheckSocialAccount(CheckSocialAccountPayload payload)
    {
        Result<UserLoginAccounts> result = null;
        try
        {
            if(payload.FacebookUserId == null && payload.GoogleUserId == null)
                return Result<UserLoginAccounts>.Error("Invalid Social");

            User? user = null;
            if (payload.FacebookUserId != null)
            {
                 user= await _db.Users.Where(x=> x.FacebookUserId == payload.FacebookUserId).FirstOrDefaultAsync();
                
            }
            else if (payload.GoogleUserId != null)
            {
                user = await _db.Users.Where(x => x.GoogleUserId == payload.GoogleUserId).FirstOrDefaultAsync();
            }
            if (user is null)
                return Result<UserLoginAccounts>.Error("Account Not Found");
            string? LatestProfileUrl = await _db.UserProfiles
                    .Where(x => x.UserId == user.UserId)
                    .OrderByDescending(p => p.CreatedDate)
                    .Select(x => x.Url)
                    .FirstOrDefaultAsync();
            string purl = "";
            if (!string.IsNullOrEmpty(LatestProfileUrl))
            {
                string bucketname = _configuration.GetSection("Buckets:UserProfile").Get<string>()!;
                Result<string> res = await _awsS3Service.GetFile(bucketname, LatestProfileUrl);
                if (res.IsSuccess)
                {
                    purl = res.Data;
                }
            }
            result = Result<UserLoginAccounts>.Success(new UserLoginAccounts
            {
                UserIdval = user.UserIdval,
                UserName = user.Name,
                UserImage = purl,
                Contact = user.Email ?? user.Phone ?? ""
            });
        }
        catch (Exception ex)
        {
            result = Result<UserLoginAccounts>.Error(ex);
        }
        return result;
    }
    public async Task<Result<ResponseUserDto>> CreateSocialAccount(SocialSignInPayload payload)
    {
        Result<ResponseUserDto> result = null;
        try
        {
            if (payload.FacebookUserId == null && payload.GoogleUserId == null)
                return Result<ResponseUserDto>.Error("Invalid Social");



            string aseKey = _configuration.GetSection("AesEncryption:AseKey").Get<string>()!;
            string aseIv = _configuration.GetSection("AesEncryption:AseIV").Get<string>()!;
            if (payload.Password is null)
                return Result<ResponseUserDto>.Error("Invalid Password");

            if (payload.FacebookUserId is not null)
            {
                var auser = await _db.Users.Where(x => x.FacebookUserId == payload.FacebookUserId).FirstOrDefaultAsync();
                if(auser is not null)
                    return Result<ResponseUserDto>.Error("Have Been Created");
                //if (!(await checkEmailUnique(requestUser.Email ?? "")))
                //    return Result<string>.Error("Your Email Have Been Registered");
                string payloadpassword = AesEncryption.Decrypt(payload.Password, aseKey, aseIv);

                // Generate a new 12-character password with at least 1 non-alphanumeric character.
                RandomPassword passwordGenerator = new RandomPassword();
                string salt = SaltedHash.GenerateSalt();
                string password = SaltedHash.ComputeHash(salt, payloadpassword.ToString());
                string userIdval = 
                    Encryption.EncryptID(payload.UserName, salt) + 
                    passwordGenerator.CreatePassword(payload.UserName.Length, payload.UserName.Length / 3);

                User user = new User
                {
                    UserIdval = userIdval,
                    Name = payload.UserName,
                    Email = payload.Email,
                    Password = password,
                    PasswordHash = salt,
                    Phone = payload.Phone,
                    DeviceId = payload.DeviceId,
                    DateCreated = DateTime.UtcNow,
                    ModifiedDate = DateTime.UtcNow,
                    Inactive = false,
                    FacebookUserId = payload.FacebookUserId,
                };
                var newUserEntry = await _db.Users.AddAsync(user);
                int result1 = await _db.SaveChangesAsync();
                if (result1 == 0 || newUserEntry.Entity == null)
                    return Result<ResponseUserDto>.Error("Registration Failed");

                // Prepare the success response model
                result = Result<ResponseUserDto>.Success(new ResponseUserDto
                {
                    UserIdval = newUserEntry.Entity.UserIdval,
                    Name = newUserEntry.Entity.Name!,
                    Password = AesEncryption.Encrypt(payloadpassword, aseKey, aseIv)
                });
            }
            else if(payload.GoogleUserId is not null)
            {
                var auser = await _db.Users.Where(x => x.GoogleUserId == payload.GoogleUserId).FirstOrDefaultAsync();
                if (auser is not null)
                    return Result<ResponseUserDto>.Error("Have Been Created");
                //if (!(await checkEmailUnique(requestUser.Email ?? "")))
                //    return Result<string>.Error("Your Email Have Been Registered");
                string payloadpassword = AesEncryption.Decrypt(payload.Password, aseKey, aseIv);

                // Generate a new 12-character password with at least 1 non-alphanumeric character.
                RandomPassword passwordGenerator = new RandomPassword();
                string salt = SaltedHash.GenerateSalt();
                string password = SaltedHash.ComputeHash(salt, payloadpassword.ToString());
                string userIdval =
                    Encryption.EncryptID(payload.UserName, salt) +
                    passwordGenerator.CreatePassword(payload.UserName.Length, payload.UserName.Length / 3);

                User user = new User
                {
                    UserIdval = userIdval,
                    Name = payload.UserName,
                    Email = payload.Email,
                    Password = password,
                    PasswordHash = salt,
                    Phone = payload.Phone,
                    DeviceId = payload.DeviceId,
                    DateCreated = DateTime.UtcNow,
                    ModifiedDate = DateTime.UtcNow,
                    Inactive = false,
                    GoogleUserId = payload.GoogleUserId,
                };
                var newUserEntry = await _db.Users.AddAsync(user);
                int result1 = await _db.SaveChangesAsync();
                if (result1 == 0 || newUserEntry.Entity == null)
                    return Result<ResponseUserDto>.Error("Registration Failed");

                // Prepare the success response model
                result= Result<ResponseUserDto>.Success(new ResponseUserDto
                {
                    UserIdval = newUserEntry.Entity.UserIdval,
                    Name = newUserEntry.Entity.Name!,
                    Password = AesEncryption.Encrypt(payloadpassword, aseKey, aseIv)
                });
            }
            else
            {
                result = Result<ResponseUserDto>.Error("Invalid Social");
            }
        }
        catch (Exception ex)
        {
            result = Result<ResponseUserDto>.Error(ex);
        }
        return result;
    }
}
