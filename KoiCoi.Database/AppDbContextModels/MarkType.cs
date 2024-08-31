using System;
using System.Collections.Generic;

namespace KoiCoi.Database.AppDbContextModels;

public partial class MarkType
{
    public int MarkTypeId { get; set; }

    public string TypeName { get; set; } = null!;

    public string? TypeDescription { get; set; }
}
