using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KoiCoi.Models.ChannelDtos.ResponseDtos;

public partial class VisitChannelResponse
{
    public string? ChannelIdval { get; set; }
    public string? ChannelName { get; set; }
    public string? ChannelDescription { get; set; }
    public bool? IsMember { get; set; }
    public string? MemberStatus { get; set; }
    public string? ChannelType { get; set; }
    public string? CreatorIdval { get; set; }
    public string? CreatorName { get; set; }
    public string? ISOCode { get; set; }
    public long? MemberCount { get; set; }
    public decimal? TotalBalance { get; set; }
    public decimal? LastBalance { get; set; }
    public string? ChannelProfile { get; set; }
                                        
}
