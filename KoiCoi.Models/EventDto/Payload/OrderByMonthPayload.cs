namespace KoiCoi.Models.EventDto.Payload;

public partial class OrderByMonthPayload
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public string? Month { get; set; }
    public string? Status { get; set; } ///Status List Is( active , last ,upcoming)
    public string? Idval { get; set; }
}
