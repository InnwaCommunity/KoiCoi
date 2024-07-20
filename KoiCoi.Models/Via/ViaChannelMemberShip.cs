using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KoiCoi.Models.Via;

public partial class ViaChannelMemberShip
{
    public int MembershipId { get; set; }

    public int ChannelId { get; set; }

    public int UserId { get; set; }

    public int UserTypeId { get; set; }

    public int StatusId { get; set; }

    public DateTime JoinedDate { get; set; }
}
