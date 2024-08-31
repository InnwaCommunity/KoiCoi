using System;
using System.Collections.Generic;

namespace KoiCoi.Database.AppDbContextModels;

public partial class Mark
{
    public int MarkId { get; set; }

    public string MarkName { get; set; } = null!;

    public string MarkSymbol { get; set; } = null!;

    public string Isocode { get; set; } = null!;

    public int? MarkTypeId { get; set; }
}
