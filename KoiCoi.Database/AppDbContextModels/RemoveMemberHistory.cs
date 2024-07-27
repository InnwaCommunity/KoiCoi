using System;
using System.Collections.Generic;

namespace KoiCoi.Database.AppDbContextModels;

public partial class RemoveMemberHistory
{
    public int HistoryId { get; set; }

    public int AdminId { get; set; }

    public int MemberId { get; set; }

    public int ChannelId { get; set; }

    public string? Reason { get; set; }

    public DateTime RemoveDate { get; set; }
}
