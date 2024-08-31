namespace KoiCoi.Models.ChannelDtos.ResponseDtos;

public partial class ChannelDataResponse
{
    public string? ChannelIdval { get; set; }
    public string? ChannelName { get; set; }
    public string? ChannelDescription { get; set; }
    public string? ChannelType { get; set; }
    public string? ISOCode { get; set; }
    public long? MemberCount { get; set; }
    public decimal? TotalBalance { get; set; }
    public decimal? LastBalance { get; set; }
}
