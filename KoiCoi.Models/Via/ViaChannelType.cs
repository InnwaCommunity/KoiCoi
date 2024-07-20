using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KoiCoi.Models.Via;

public partial class ViaChannelType
{
    public int ChannelTypeId { get; set; }

    public string ChannelTypeName { get; set; } = null!;

    public string ChannelTypeDescription { get; set; } = null!;
}
