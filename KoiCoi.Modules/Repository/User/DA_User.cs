
using KoiCoi.Database.AppDbContextModels;
using KoiCoi.Models.Via;
using Microsoft.Extensions.Configuration;
using Org.BouncyCastle.Crypto;
using Serilog;
using System.ComponentModel.DataAnnotations;

namespace KoiCoi.Modules.Repository.User;

public class DA_User
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _configuration;

    public DA_User(AppDbContext db, IConfiguration configuration)
    {
        _db = db;
        _configuration = configuration;
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
            RequestUserDto? userData = await _db.Users.Where(x => x.Name == viaUser.Name && x.Password == viaUser.Password)
                .Select(x => new RequestUserDto
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

    public async Task<Result<string>> UpdateUserInfo(RequestUserDto requestUserDto,int LoginUserId)
    {
        Result<string> model = null;
        try
        {
            var useData = await _db.Users
                                        .Where(x => x.UserId == LoginUserId).FirstOrDefaultAsync();
            if (useData == null)
                return Result<string>.Error("User Not Found");
            useData.Name = requestUserDto.Email ?? useData.Email!;
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
            List<UserInfoResponse> user = await _db.Users.Where(x => x.Name.Contains(name) && x.Inactive == false)
                                           .Select(x=> new UserInfoResponse
                                           {
                                               UserIdval = Encryption.EncryptID(x.UserId.ToString(), LoginUserId.ToString()),
                                               UserName = x.Name,
                                           })
                                          .ToListAsync();

            model = Result<List<UserInfoResponse>>.Success(user);
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
            if (string.IsNullOrEmpty(payload.description)) return Result<string>.Error("Image Not Found");

            string? folderPath = _configuration["appSettings:UserProfile"];
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


            UserProfile profile = new UserProfile
            {
                Url = filename,
                UrlDescription = payload.description,
                UserId = LoginUserId,
                CreatedDate = DateTime.UtcNow,
            };

            await _db.UserProfiles.AddAsync(profile);
            await _db.SaveChangesAsync();
            model = Result<string>.Success("Upload Success");

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
}
