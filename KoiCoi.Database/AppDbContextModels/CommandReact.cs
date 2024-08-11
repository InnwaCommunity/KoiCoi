using System;
using System.Collections.Generic;

namespace KoiCoi.Database.AppDbContextModels;

public partial class CommandReact
{
    public int ReactId { get; set; }

    public int CommandId { get; set; }

    public int UserId { get; set; }

    public int ReactTypeId { get; set; }

    public DateTime? CreatedDate { get; set; }
}
