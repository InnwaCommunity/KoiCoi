using System;
using System.Collections.Generic;

namespace KoiCoi.Database.AppDbContextModels;

public partial class EventFile
{
    public int UrlId { get; set; }

    public string? Url { get; set; }

    public string? UrlDescription { get; set; }

    public int EventId { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public string? Extension { get; set; }
}
