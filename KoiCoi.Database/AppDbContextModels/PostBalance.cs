using System;
using System.Collections.Generic;

namespace KoiCoi.Database.AppDbContextModels;

public partial class PostBalance
{
    public int BalanceId { get; set; }

    public int PostId { get; set; }

    public string Balance { get; set; } = null!;

    public int MarkId { get; set; }
}
