using System;
using System.Collections.Generic;

namespace KoiCoi.Database.AppDbContextModels;

public partial class CommandViewer
{
    public int ViewId { get; set; }

    public int CommandId { get; set; }

    public int UserId { get; set; }

    public DateTime? CreatedDate { get; set; }
}
