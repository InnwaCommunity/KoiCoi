using System;
using System.Collections.Generic;

namespace KoiCoi.Database.AppDbContextModels;

public partial class VisitChannelHistory
{
    public int HistoryId { get; set; }

    public int UserId { get; set; }

    public int InviterId { get; set; }

    public int ChannelId { get; set; }

    public DateTime ViewedDate { get; set; }
}
