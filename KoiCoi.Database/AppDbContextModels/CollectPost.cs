using System;
using System.Collections.Generic;

namespace KoiCoi.Database.AppDbContextModels;

public partial class CollectPost
{
    public int PostId { get; set; }

    public string? Content { get; set; }

    public string? CollectAmount { get; set; }

    public int CreaterId { get; set; }

    public int EventId { get; set; }

    public int ApproverId { get; set; }

    public int PrivacyId { get; set; }

    public int StatusId { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public bool? Inactive { get; set; }
}
