namespace KoiCoi.Models.EventDto.Payload;

public partial class OrderByMonthPayload
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public string? Month { get; set; }
    public string? Idval { get; set; }
}
