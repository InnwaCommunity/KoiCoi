using System;
using System.Collections.Generic;

namespace KoiCoi.Database.AppDbContextModels;

public partial class ChannelBalanceRecord
{
    public int BalanceRecordId { get; set; }

    public int ChannelId { get; set; }

    public string TotalBalance { get; set; } = null!;

    public string LastBalance { get; set; } = null!;

    public DateTime CreatedBalance { get; set; }
}
