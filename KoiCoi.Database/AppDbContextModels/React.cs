using System;
using System.Collections.Generic;

namespace KoiCoi.Database.AppDbContextModels;

public partial class React
{
    public int ReactId { get; set; }

    public int PostId { get; set; }

    public int UserId { get; set; }

    public int ReactTypeId { get; set; }

    public DateTime? CreatedDate { get; set; }
}
