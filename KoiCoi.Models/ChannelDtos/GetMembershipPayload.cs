using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KoiCoi.Models.ChannelDtos;

public partial class GetMembershipPayload
{
    public string? ChannelIdval { get; set; }
    public string? MemberState { get; set; }
}
