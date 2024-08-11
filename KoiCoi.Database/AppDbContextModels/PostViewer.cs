using System;
using System.Collections.Generic;

namespace KoiCoi.Database.AppDbContextModels;

public partial class PostViewer
{
    public int ViewerId { get; set; }

    public int PostId { get; set; }

    public int UserId { get; set; }

    public DateTime? CreatedDate { get; set; }
}
