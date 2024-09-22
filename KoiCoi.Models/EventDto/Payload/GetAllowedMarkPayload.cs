

namespace KoiCoi.Models.EventDto.Payload;

public class GetAllowedMarkPayload
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string MarkTypeIdval { get; set; } = string.Empty;
    public string EventIdval { get; set; }
}
