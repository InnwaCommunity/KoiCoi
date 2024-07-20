using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KoiCoi.Models.ChannelDtos;

public partial class ChannelTypePayloads
{
    public string? ChannelTypeIdval { get; set; }

    public string? ChannelTypeName { get; set; }

    public string? ChannelTypeDescription { get; set; }
}

public partial class ChannelTypeResponseDto
{
    public string? ChannelTypeIdval { get; set; }

    public string? ChannelTypeName { get; set; }

    public string? ChannelTypeDescription { get; set; }
}