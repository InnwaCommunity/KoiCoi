namespace KoiCoi.Models.ChannelDtos;

public partial class ChannelInviteLinkPayload
{
    public string? InviteLink { get; set; }
}

public partial class JoinChannelInviteLinkPayload
{
    public string? InviteLink { get; set; }
    public string? ChannelIdval { get; set; }
    public bool? IsJoin { get; set; }
}
