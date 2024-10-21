namespace KoiCoi.Models.EventDto.Payload;

public partial class OverallContributionPayload
{
    public string Idval { get; set; } = string.Empty;
    public string? MarkIdval { get; set; }
    public int pageNumber { get; set; } = 1;
    public int pageSize { get; set; } = 5;
}
