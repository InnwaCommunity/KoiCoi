using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KoiCoi.Models.ChannelDtos.ResponseDtos;
public partial class VisitUserResponse
{
    public string? UserIdval { get; set; }
    public string? UserName { get; set; }
    public string? InviterIdval { get; set; }
    public string? InviterName { get; set; }
    public string? VisitedDate { get; set; }
}
