using System;
using System.Collections.Generic;

namespace KoiCoi.Database.AppDbContextModels;

public partial class Post
{
    public int PostId { get; set; }

    public string? Content { get; set; }

    public int EventId { get; set; }

    public int? TagId { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public bool? Inactive { get; set; }
}
