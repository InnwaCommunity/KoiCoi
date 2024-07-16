using System;
using System.Collections.Generic;

namespace KoiCoi.Database.AppDbContextModels;

public partial class StatusType
{
    public int StatusId { get; set; }

    public string StatusName { get; set; } = null!;

    public string StatusDescription { get; set; } = null!;
}
