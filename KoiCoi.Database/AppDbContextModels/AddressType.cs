using System;
using System.Collections.Generic;

namespace KoiCoi.Database.AppDbContextModels;

public partial class AddressType
{
    public int AddressId { get; set; }

    public string Address { get; set; } = null!;

    public string? Description { get; set; }
}
