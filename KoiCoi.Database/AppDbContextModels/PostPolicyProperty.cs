using System;
using System.Collections.Generic;

namespace KoiCoi.Database.AppDbContextModels;

public partial class PostPolicyProperty
{
    public int PropertyId { get; set; }

    public int PostId { get; set; }

    public int PolicyId { get; set; }

    public int? MaxCount { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public bool? GroupMemberOnly { get; set; }

    public bool? FriendOnly { get; set; }
}
