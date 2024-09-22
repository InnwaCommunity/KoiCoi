

namespace KoiCoi.Models.EventDto;

public partial class GetEventRequestPayload
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? ChannelIdval { get; set; }
    public string? Status { get; set; }
}
