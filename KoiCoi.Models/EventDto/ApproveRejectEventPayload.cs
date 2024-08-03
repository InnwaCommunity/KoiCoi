using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KoiCoi.Models.EventDto;
public partial class ApproveRejectEventPayload
{
    public string? EventIdval { get; set; }
    public int? Status { get; set; }
}
