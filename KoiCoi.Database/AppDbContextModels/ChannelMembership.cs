using System;
using System.Collections.Generic;

namespace KoiCoi.Database.AppDbContextModels;

public partial class ChannelMembership
{
    public int MembershipId { get; set; }

    public int ChannelId { get; set; }

    public int UserId { get; set; }

    public int UserTypeId { get; set; }

    public int StatusId { get; set; }

    public DateTime JoinedDate { get; set; }

    public int? InviterId { get; set; }
}
