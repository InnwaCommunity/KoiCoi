
namespace KoiCoi.Models.ChannelDtos.ResponseDtos;

public partial class JoinChannelResponse
{
    public string? ChannelIdval { get; set; }
    public string? ChannelName { get; set; }
    public string? ChannelDescription { get; set; }
    public bool? IsMember { get; set; }
    public string? MemberStatus { get; set; }
    public string? ChannelType { get; set; }
    public long? MemberCount { get; set; }
}
