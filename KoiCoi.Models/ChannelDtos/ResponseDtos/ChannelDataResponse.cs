namespace KoiCoi.Models.ChannelDtos.ResponseDtos;

public partial class ChannelDataResponse
{
    public string? ChannelIdval { get; set; }
    public string? ChannelName { get; set; }
    public string? ChannelDescription { get; set; }
    public string? ChannelType { get; set; }
    public long? MemberCount { get; set; }
    public string? ChannelProfile { get; set; }
    public List<ChannelBalanceData> BalanceDatas { get; set; }   = new List<ChannelBalanceData>();
}

public class ChannelBalanceData
{
    public string? MarkIdval { get; set; }
    public string? MarkName { get; set; }
    public string? IsoCode { get; set; }
    public decimal TotalBalance { get; set; }
    public decimal LastBalance { get; set; }
}

