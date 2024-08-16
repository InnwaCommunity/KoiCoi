using System;
using System.Collections.Generic;

namespace KoiCoi.Database.AppDbContextModels;

public partial class AccountLoginHistory
{
    public int HistoryId { get; set; }

    public int UserId { get; set; }

    public string? DeviceId { get; set; }

    public string? AppVersion { get; set; }

    public string? OsVersion { get; set; }

    public string? PhoneModel { get; set; }

    public DateTime CreatedData { get; set; }

    public DateTime ModifiedData { get; set; }
}
