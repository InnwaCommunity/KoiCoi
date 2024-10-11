
namespace KoiCoi.Models.PostDtos.Response;

public partial class GetCommentResponse
{
    public string CommandIdval { get; set; } = string.Empty;
    public string? Content { get; set; } = string.Empty;
    public string CreatorIdval { get; set; } = string.Empty;
    public string CreatorName { get; set; } = string.Empty;
    public string? CreatorEmail { get; set; }
    public bool CanEdit { get; set; } = false;
    public DateTime? CreateData { get; set; }
    public int ReactCount { get; set; }
    public bool Selected { get; set; }= false;
    public string? CreatorImage { get; set; }
    public bool HaveChildCommand { get; set; } = false;
}
