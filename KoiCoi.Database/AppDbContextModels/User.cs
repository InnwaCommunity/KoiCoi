using System;
using System.Collections.Generic;

namespace KoiCoi.Database.AppDbContextModels;

public partial class User
{
    public int UserId { get; set; }

    public string UserIdval { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Email { get; set; }

    public string Password { get; set; } = null!;

    public string? Phone { get; set; }

    public string PasswordHash { get; set; } = null!;

    public string? DeviceId { get; set; }

    public DateTime DateCreated { get; set; }

    public DateTime ModifiedDate { get; set; }

    public bool? Inactive { get; set; }
}
