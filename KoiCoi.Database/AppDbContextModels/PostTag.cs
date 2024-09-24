using System;
using System.Collections.Generic;

namespace KoiCoi.Database.AppDbContextModels;

public partial class PostTag
{
    public int PostTagId { get; set; }

    public int PostId { get; set; }

    public int? EventTagId { get; set; }

    public int? UserId { get; set; }
}
