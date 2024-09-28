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

    public int? ChannelId { get; set; }

    public int? UserId { get; set; }

    public virtual Channel? Channel { get; set; }

    public virtual ICollection<ExchangeRate> ExchangeRateFromMarks { get; set; } = new List<ExchangeRate>();

    public virtual ICollection<ExchangeRate> ExchangeRateToMarks { get; set; } = new List<ExchangeRate>();

    public virtual User? User { get; set; }
}
