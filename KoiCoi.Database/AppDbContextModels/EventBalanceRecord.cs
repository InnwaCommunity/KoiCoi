using System;
using System.Collections.Generic;

namespace KoiCoi.Database.AppDbContextModels;

public partial class EventBalanceRecord
{
    public int BalanceRecordId { get; set; }

    public int EventId { get; set; }

    public string? TotalBalance { get; set; }

    public string? LastBalance { get; set; }

    public DateTime? CreatedDate { get; set; }
}
