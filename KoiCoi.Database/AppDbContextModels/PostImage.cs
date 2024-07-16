using System;
using System.Collections.Generic;

namespace KoiCoi.Database.AppDbContextModels;

public partial class PostImage
{
    public int UrlId { get; set; }

    public string Url { get; set; } = null!;

    public string? Description { get; set; }

    public int PostId { get; set; }

    public DateTime? CreatedDate { get; set; }
}
