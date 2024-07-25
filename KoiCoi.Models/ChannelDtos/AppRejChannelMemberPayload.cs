using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KoiCoi.Models.ChannelDtos;

public partial class AppRejChannelMemberPayload
{
    public string? MembershipIdval { get; set; }
    public string? UserTypeIdval { get; set; }
    public int? ApproveStatus { get; set; } ///1 is approve and 2 is reject
}
