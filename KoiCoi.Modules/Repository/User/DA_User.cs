
using System.ComponentModel.DataAnnotations;

namespace KoiCoi.Modules.Repository.User;

public class DA_User
{
    private readonly AppDbContext _db;

    public DA_User(AppDbContext db)
    {
        _db = db;
    }

    public async Task<ResponseData> CreateAccount(RequestUserDto requestUserDto,string temppassword)
    {
        ResponseData responseData = new ResponseData();
        try
        {
            await _db.Users.AddAsync(requestUserDto.ChangeUser());
            int result = await _db.SaveChangesAsync();
            if (result == 0)
                throw new ValidationException("Registration Fail");
            RequestUserDto? userData = await _db.Users.Where(x => x.Name == requestUserDto.Name && x.Password == requestUserDto.Password)
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

    public async Task<ResponseData> UpdateUserInfo(RequestUserDto requestUserDto)
    {
        ResponseData responseData = new ResponseData();
        try
        {
            RequestUserDto? useData = await _db.Users
                                        .Where(x => x.UserId == requestUserDto.Id)
                                        .Select(x=> new RequestUserDto
                                        {
                                            Id= x.UserId,
                                            UserIdval = x.UserIdval,
                                            Name = x.Name,
                                            Email = x.Email,
                                            Phone = x.Phone,
                                            DeviceId = x.DeviceId
                                        })
                                        .FirstOrDefaultAsync();
            if (useData==null)
                throw new ValidationException("User Not Found");
            useData.Name = requestUserDto.Email ?? useData.Email;
            useData.Phone = requestUserDto.Phone ?? useData.Phone;
            useData.DeviceId = requestUserDto.DeviceId ?? useData.DeviceId;
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

    public async Task<ResponseData> FindUserByIdval(string idval)
    {
        ResponseData responseData = new ResponseData();
        try
        {
            var userData = await _db.Users.Where(x=> x.UserIdval == idval).FirstOrDefaultAsync();
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
}
