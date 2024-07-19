using System;
using System.Collections.Generic;

namespace KoiCoi.Database.AppDbContextModels;

public partial class InformMail
{
    public int MailId { get; set; }

    public string FromMail { get; set; } = null!;

    public string AppPassword { get; set; } = null!;

    public int? UseCount { get; set; }

    public DateTime? DateCreated { get; set; }
}
