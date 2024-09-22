using System;
using System.Collections.Generic;

namespace KoiCoi.Database.AppDbContextModels;

public partial class EventAllowedMark
{
    public int AllowedMarkId { get; set; }

    public string AllowedMarkName { get; set; } = null!;

    public int MarkId { get; set; }

    public int EventPostId { get; set; }
}
