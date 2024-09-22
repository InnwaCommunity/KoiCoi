namespace KoiCoi.Models.EventDto.Payload;

public partial class EventContributionPayload
{
    public string EventIdval { get; set; } = string.Empty;
    public string MarkIdval { get; set; } = string.Empty;
    public int pageNumber { get; set; }
    public int pageSize { get; set; }
}
