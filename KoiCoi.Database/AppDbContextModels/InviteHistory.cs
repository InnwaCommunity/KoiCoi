using System;
using System.Collections.Generic;

namespace KoiCoi.Database.AppDbContextModels;

public partial class InviteHistory
{
    public int InviteId { get; set; }

    public int InviteUserId { get; set; }

    public int JoinedUserId { get; set; }

    public int ChannelId { get; set; }

    public string InviteData { get; set; } = null!;

    public DateTime? JoinedDate { get; set; }
}
