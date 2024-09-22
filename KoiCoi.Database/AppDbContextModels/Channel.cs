using System;
using System.Collections.Generic;

namespace KoiCoi.Database.AppDbContextModels;

public partial class Channel
{
    public int ChannelId { get; set; }

    public string ChannelName { get; set; } = null!;

    public string? StatusDescription { get; set; }

    public int ChannelType { get; set; }

    public int CreatorId { get; set; }

    public long MemberCount { get; set; }

    public DateTime DateCreated { get; set; }

    public DateTime ModifiedDate { get; set; }

    public bool? Inactive { get; set; }
}
