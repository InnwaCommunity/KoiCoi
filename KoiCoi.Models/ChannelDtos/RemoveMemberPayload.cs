using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KoiCoi.Models.ChannelDtos;

public partial class RemoveMemberPayload
{
    public string? ChannelIdval { get; set; }
    public List<RemoveMemberData>? removeMember { get; set; }
}

public partial class RemoveMemberData
{
    public string? MemberIdval { get; set; }
    public string? Reason { get; set; }
}
