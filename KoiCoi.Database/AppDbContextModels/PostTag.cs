using System;
using System.Collections.Generic;

namespace KoiCoi.Database.AppDbContextModels;

public partial class PostTag
{
    public int TagId { get; set; }

    public string TagName { get; set; } = null!;

    public string? TagDescription { get; set; }

    public int EventPostId { get; set; }

    public int CreatorId { get; set; }

    public DateTime? CreateDate { get; set; }

    public bool? Inactive { get; set; }
}
