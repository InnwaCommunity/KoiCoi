using System;
using System.Collections.Generic;

namespace KoiCoi.Database.AppDbContextModels;

public partial class UserPostInteraction
{
    public int InteractionId { get; set; }

    public int PostId { get; set; }

    public int UserId { get; set; }

    public string ViewedContext { get; set; } = null!;

    public string? PostType { get; set; }

    public decimal VisibilityPercentage { get; set; }

    public bool? Like { get; set; }

    public bool? Comment { get; set; }

    public bool? Share { get; set; }

    public int? InteractionDuration { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }
}
