using System;
using System.Collections.Generic;

namespace KoiCoi.Database.AppDbContextModels;

public partial class NotificationToken
{
    public int TokenId { get; set; }

    public string Token { get; set; } = null!;

    public int UserId { get; set; }

    public DateTime LastActivities { get; set; }

    public string AppVersion { get; set; } = null!;

    public string OsVersion { get; set; } = null!;

    public string PhModel { get; set; } = null!;

    public bool? IsRooted { get; set; }
}
