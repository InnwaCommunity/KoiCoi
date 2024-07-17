using System;
using System.Collections.Generic;

namespace KoiCoi.Database.AppDbContextModels;

public partial class User
{
    public int UserId { get; set; }

    public string Name { get; set; } = null!;

    public string? Email { get; set; }

    public string? Password { get; set; }

    public string? Phone { get; set; }

    public string? PasswordHash { get; set; }

    public string DeviceId { get; set; } = null!;

    public DateTime? DateCreated { get; set; }
}
