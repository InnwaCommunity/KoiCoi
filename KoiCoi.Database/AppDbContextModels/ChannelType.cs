using System;
using System.Collections.Generic;

namespace KoiCoi.Database.AppDbContextModels;

public partial class ChannelType
{
    public int ChannelTypeId { get; set; }

    public string ChannelTypeName { get; set; } = null!;

    public string ChannelTypeDescription { get; set; } = null!;
}
