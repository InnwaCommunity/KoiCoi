using System;
using System.Collections.Generic;

namespace KoiCoi.Database.AppDbContextModels;

public partial class ReactType
{
    public int TypeId { get; set; }

    public string Description { get; set; } = null!;

    public string? Icon { get; set; }
}
