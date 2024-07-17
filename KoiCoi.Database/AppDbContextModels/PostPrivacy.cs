using System;
using System.Collections.Generic;

namespace KoiCoi.Database.AppDbContextModels;

public partial class PostPrivacy
{
    public int PrivacyId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }
}
