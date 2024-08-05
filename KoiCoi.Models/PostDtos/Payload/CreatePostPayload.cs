namespace KoiCoi.Models.PostDtos.Payload;

public partial class CreatePostPayload
{
    public string? Content { get; set; }
    public string? EventIdval { get; set; }
    public string? PrivacyIdval { get; set; }
    public decimal CollectAmount { get; set; }
    public List<PostImagePayload>? PostImageData { get; set; }
}
