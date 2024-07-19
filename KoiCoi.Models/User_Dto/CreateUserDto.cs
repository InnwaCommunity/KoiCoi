using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KoiCoi.Models.User_Dto;

public class ResponseUserDto
{

    public string UserIdval { get; set; } = null!;
    public string Name { get; set; } = null!;

    public string Password { get; set; } = null!;
}

public class RequestUserDto
{
    public int? Id { get; set; }
    public string? Name { get; set; }
    public string? UserIdval { get; set; }

    public string? Email { get; set; }

    public string? Password { get; set; }

    public string? Phone { get; set; }

    public string? PasswordHash { get; set; }

    public string? DeviceId { get; set; }

    public DateTime? DateCreated { get; set; }
}
