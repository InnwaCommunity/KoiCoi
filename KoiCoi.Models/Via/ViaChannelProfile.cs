using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KoiCoi.Models.Via;

public partial class ViaChannelProfile
{
    public int ProfileId { get; set; }

    public string Url { get; set; } = null!;

    public string? UrlDescription { get; set; }

    public int ChannelId { get; set; }

    public DateTime? CreatedDate { get; set; }
}
