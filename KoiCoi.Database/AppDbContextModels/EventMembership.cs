using System;
using System.Collections.Generic;

namespace KoiCoi.Database.AppDbContextModels;

public partial class EventMembership
{
    public int MembershipId { get; set; }

    public int EventId { get; set; }

    public int UserId { get; set; }

    public int UserTypeId { get; set; }
}
