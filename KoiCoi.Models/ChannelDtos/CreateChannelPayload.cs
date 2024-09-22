using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KoiCoi.Models.ChannelDtos;


public partial class CreateChannelReqeust
{
    public string ChannelName { get; set; } = null!;

    public string? StatusDescription { get; set; }

    public string? ChannelTypeval { get; set; }

    public string? ProImage64 { get; set; }
    public string? ImageExt { get; set; }

    public string? imagedescription { get; set; }
}
