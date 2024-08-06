using System;
using System.Collections.Generic;

namespace KoiCoi.Database.AppDbContextModels;

public partial class Policy
{
    public int PolicyId { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;
}
