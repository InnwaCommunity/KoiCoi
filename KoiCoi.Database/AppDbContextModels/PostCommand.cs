using System;
using System.Collections.Generic;

namespace KoiCoi.Database.AppDbContextModels;

public partial class PostCommand
{
    public int CommandId { get; set; }

    public string? Content { get; set; }

    public int PostId { get; set; }

    public int UserId { get; set; }

    public int? ParentCommandId { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public DateTime? CreatedDate { get; set; }
}
