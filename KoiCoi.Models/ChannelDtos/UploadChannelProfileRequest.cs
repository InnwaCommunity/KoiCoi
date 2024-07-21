using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KoiCoi.Models.ChannelDtos;

public partial class UploadChannelProfileRequest
{
    public string? ChannelIdval { get; set; }
    public string? base64data { get; set; }
    public string? description { get; set; }
}
