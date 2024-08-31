using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KoiCoi.Models.Via;


public partial class ViaChannel
{
    public int ChannelId { get; set; }

    public string ChannelName { get; set; } = null!;

    public string? StatusDescription { get; set; }

    public int ChannelType { get; set; }

    public int CreatorId { get; set; }

    public int MarkId { get; set; }

    public long MemberCount { get; set; }

    public string? TotalBalance { get; set; }

    public string? LastBalance { get; set; }

    public DateTime DateCreated { get; set; }

    public DateTime ModifiedDate { get; set; }

    public bool? Inactive { get; set; }
}
