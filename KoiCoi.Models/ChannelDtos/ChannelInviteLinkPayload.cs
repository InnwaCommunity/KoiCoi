using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KoiCoi.Models.ChannelDtos;

public partial class ChannelInviteLinkPayload
{
    public string? InviteLink { get; set; }
}

public partial class JoinChannelInviteLinkPayload
{
    public string? InviteLink { get; set; }
    public bool? IsJoin { get; set; }
}
