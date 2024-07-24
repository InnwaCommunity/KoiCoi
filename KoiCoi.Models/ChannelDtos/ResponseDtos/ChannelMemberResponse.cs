using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KoiCoi.Models.ChannelDtos.ResponseDtos;

public partial class ChannelMemberResponse
{
    public string? MembershipId { get; set; }
    public string? MemberIdval { get; set; }
    public string? MemberName { get; set; }
    public string? UserTypeIdval { get; set; }
    public string? UserTypeName { get; set; }
    public string? InviterIdval { get; set; }
    public string? InviterName { get; set; }
    public DateTime JoinedDate { get; set; }
    public string? UserImage64 { get; set; }
}
