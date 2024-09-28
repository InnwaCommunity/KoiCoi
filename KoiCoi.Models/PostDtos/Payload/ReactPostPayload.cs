

namespace KoiCoi.Models.PostDtos.Payload;

public partial class ReactPostPayload
{
    public string? postIdval { get; set; }
    public string? reacttypeIdval { get; set; }
}


public partial class CommentPostPayload
{
    public string? PostIdval { get; set; } = null;
    public string? Content { get; set; } = null;
    public string? ParentIdval { get; set; } = null;
}