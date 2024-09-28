using System;
using System.Collections.Generic;

namespace KoiCoi.Database.AppDbContextModels;

public partial class ExchangeRate
{
    public int ExchangeRateId { get; set; }

    public int FromMarkId { get; set; }

    public int ToMarkId { get; set; }

    public int EventPostId { get; set; }

    public decimal MinQuantity { get; set; }

    public decimal Rate { get; set; }

    public virtual Post EventPost { get; set; } = null!;

    public virtual Mark FromMark { get; set; } = null!;

    public virtual Mark ToMark { get; set; } = null!;
}
