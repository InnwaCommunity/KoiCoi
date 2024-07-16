using System;
using System.Collections.Generic;

namespace KoiCoi.Database.AppDbContextModels;

public partial class ChannelProfile
{
    public int ProfileId { get; set; }

    public string Url { get; set; } = null!;

    public string? UrlDescription { get; set; }

    public int ChannelId { get; set; }

    public DateTime? CreatedDate { get; set; }
}
