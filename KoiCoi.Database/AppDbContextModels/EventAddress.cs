using System;
using System.Collections.Generic;

namespace KoiCoi.Database.AppDbContextModels;

public partial class EventAddress
{
    public int EventAddressId { get; set; }

    public int AddressId { get; set; }

    public int EventId { get; set; }

    public string? AddressName { get; set; }
}
