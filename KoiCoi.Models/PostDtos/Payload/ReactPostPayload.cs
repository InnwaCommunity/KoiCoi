﻿

namespace KoiCoi.Models.PostDtos.Payload;

public partial class ReactPostPayload
{
    public string? postIdval { get; set; }
    public string? reacttypeIdval { get; set; }
}


public partial class ReactCommentPayload
{
    public string? commentIdval { get; set; }
    public string? reacttypeIdval { get; set; }
}
public partial class CommentPostPayload
{
    public string? PostIdval { get; set; } = null;
    public string? Content { get; set; } = null;
    public string? ParentIdval { get; set; } = null;
}

public partial class GetCommentPayload
{
    public string PostIdval { get; set; } = string.Empty;
    public string? ParentCommandIdval { get; set; }
    public int pageNumber { get; set;}
    public int pageSize { get; set;}
}

public partial class UpdateCommentPayload
{
    public string CommentIdval { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}

public partial class DeleteCommentPayload
{
    public string CommentIdval { get; set; } = string.Empty;
}