namespace KoiCoi.Models.PostDtos.Payload;

public partial class GetEachUserPostsPayload
{
    public string? UserIdval { get; set; }
    public int pageNumber { get; set; }
    public int pageSize { get; set; }
    public string? Status { get; set; }
}
