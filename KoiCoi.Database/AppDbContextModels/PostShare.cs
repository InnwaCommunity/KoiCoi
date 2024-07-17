using System;
using System.Collections.Generic;

namespace KoiCoi.Database.AppDbContextModels;

public partial class PostShare
{
    public int ShareId { get; set; }

    public string? Caption { get; set; }

    public int PostId { get; set; }

    public int UserId { get; set; }

    public DateTime? CreatedDate { get; set; }
}
