using System;
using System.Collections.Generic;

namespace KoiCoi.Database.AppDbContextModels;

public partial class ChannelTopic
{
    public int TopicId { get; set; }

    public string TopicName { get; set; } = null!;

    public string? Descriptions { get; set; }

    public int ChannelId { get; set; }

    public DateTime DateCreated { get; set; }
}
