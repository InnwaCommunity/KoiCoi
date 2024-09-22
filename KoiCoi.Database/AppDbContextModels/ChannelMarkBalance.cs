using System;
using System.Collections.Generic;

namespace KoiCoi.Database.AppDbContextModels;

public partial class ChannelMarkBalance
{
    public int BalanceId { get; set; }

    public int ChannelId { get; set; }

    public int MarkId { get; set; }

    public string TotalBalance { get; set; } = null!;

    public string LastBalance { get; set; } = null!;
}
