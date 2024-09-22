namespace KoiCoi.Models.EventDto.Payload;

public partial class GetEventData
{
    public string EventIdval { get; set; } = string.Empty;
    public int pageNumber { get; set; } = 1;
    public int pageSize { get; set; } = 10;
}
