using System;
using System.Collections.Generic;

namespace KoiCoi.Database.AppDbContextModels;

public partial class UserType
{
    public int TypeId { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;
}
