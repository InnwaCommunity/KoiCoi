using KoiCoi.Mapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            if (result < 0)
            {
                responseData.StatusCode = 0;
                responseData.Message = "Registration Fail";
                return responseData;
            }
            RequestUserDto? userData = await _db.Users.Where(x => x.Name == requestUserDto.Name && x.Password == requestUserDto.Password)
                .Select(x => new RequestUserDto
                {
                    UserIdval = x.UserIdval,
                    Name = x.Name
                }).FirstOrDefaultAsync();
            if (userData == null)
            {
                responseData.StatusCode = 0;
                responseData.Message = "Registration Fail";
                return responseData;
            }
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
