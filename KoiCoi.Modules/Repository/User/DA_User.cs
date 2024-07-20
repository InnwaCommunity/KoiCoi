
using KoiCoi.Database.AppDbContextModels;
using KoiCoi.Models.Via;
using Org.BouncyCastle.Crypto;
using System.ComponentModel.DataAnnotations;

namespace KoiCoi.Modules.Repository.User;

public class DA_User
{
    private readonly AppDbContext _db;

    public DA_User(AppDbContext db)
    {
        _db = db;
    }

    public async Task<ResponseData> CreateAccount(ViaUser viaUser,string temppassword)
    {
        ResponseData responseData = new ResponseData();
        try
        {
            if (!(await checkEmailUnique(viaUser.Email ?? "")))
                throw new ValidationException("You Email Have Been Registered");
            await _db.Users.AddAsync(viaUser.ChangeUser());
            int result = await _db.SaveChangesAsync();
            if (result == 0)
                throw new ValidationException("Registration Fail");
            RequestUserDto? userData = await _db.Users.Where(x => x.Name == viaUser.Name && x.Password == viaUser.Password)
                .Select(x => new RequestUserDto
                {
                    UserIdval = x.UserIdval,
                    Name = x.Name
                }).FirstOrDefaultAsync();
            if (userData == null)
                throw new ValidationException("Registration Fail");
            responseData.StatusCode = 1;
            responseData.Message = "Registration Success";
            responseData.Data = new ResponseUserDto
            {
                    UserIdval =userData.UserIdval!,
                    Name = userData.Name!,
                    Password = temppassword!
            };
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
    private async Task<bool> checkEmailUnique(string email)
    {
        bool unique = false;
        var resultAdmin = await _db.Users.Where(x => x.Email == email).FirstOrDefaultAsync();
        if (resultAdmin == null)
        {
            unique = true;///There is no account by this email.
        }
        return unique;
    }

    public async Task<ResponseData> UpdateUserInfo(RequestUserDto requestUserDto,int LoginUserId)
    {
        ResponseData responseData = new ResponseData();
        try
        {
            var useData = await _db.Users
                                        .Where(x => x.UserId == LoginUserId).FirstOrDefaultAsync();
            if (useData==null)
                throw new ValidationException("User Not Found");
            useData.Name = requestUserDto.Email ?? useData.Email!;
            useData.Phone = requestUserDto.Phone ?? useData.Phone;
            useData.DeviceId = requestUserDto.DeviceId ?? useData.DeviceId;
            useData.ModifiedDate = DateTime.Now;
            int result = await _db.SaveChangesAsync();
            if (result == 0)
                throw new ValidationException("Update Fail");
            responseData.StatusCode = 1;
            responseData.Message = "Update Success";
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

    public async Task<ResponseData> FindUserByIdval(int userId)
    {
        ResponseData responseData = new ResponseData();
        try
        {
            var userData = await _db.Users.Where(x=> x.UserId == userId && x.Inactive == false).FirstOrDefaultAsync();
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

    public async Task<ResponseData> FindUserByName(string name,int LoginUserId)
    {
        ResponseData responseData = new ResponseData();
        try
        {
            var userData = await _db.Users.Where(x => x.Name.Contains(name) && x.Inactive == false).ToListAsync();
            if (userData == null)
                throw new ValidationException("Login User  not found.");

            var respon = new List<dynamic>();
            foreach (var item in userData)
            {
                var newres = new
                {
                    UserIdval = Encryption.EncryptID(item.UserId.ToString(), LoginUserId.ToString()),
                    Name = item.Name,
                };
                respon.Add(newres);
            }
            responseData.StatusCode = 1;
            responseData.Message = "Get User Success";
            responseData.Data = respon;
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

    public async Task<ResponseData> DeleteLoginUser(int LoginUserId)
    {
        ResponseData responseData = new ResponseData();
        try
        {
            var userData = await _db.Users.Where(x => x.UserId == LoginUserId).FirstOrDefaultAsync();
            if (userData == null)
                throw new ValidationException("Login User  not found.");

            userData.ModifiedDate = DateTime.Now;
            userData.Inactive = true;
            await _db.SaveChangesAsync();
            responseData.StatusCode = 1;
            responseData.Message = "Login User Delete Success.You can get your account within 30 days.Please remember your password or your email.";
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
}
