namespace KoiCoi.Models.EventDto.Payload;

public partial class GetUserContributonsPayload
{
    public string? UserIdval { get; set; }
    public string MarkIdval { get; set; }

    public int pageNumber { get; set; } = 1;
    public int pageSize { get; set; } = 30;
}
