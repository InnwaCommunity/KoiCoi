using KoiCoi.Models.User_Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KoiCoi.Mapper;

public static class ChangeDatabaseModel
{
    #region User

    public static User ChangeUser(this RequestUserDto requestUserDto)
    {
    User user = new User
        {
            UserIdval = requestUserDto.UserIdval!,
            Name = requestUserDto.Name!,
            Email = requestUserDto.Email,
            Password = requestUserDto.Password!,
            Phone = requestUserDto.Phone,
            PasswordHash = requestUserDto.PasswordHash,
            DeviceId = requestUserDto.DeviceId,
            DateCreated = requestUserDto.DateCreated
        };
        return user;
    }
    #endregion
}
