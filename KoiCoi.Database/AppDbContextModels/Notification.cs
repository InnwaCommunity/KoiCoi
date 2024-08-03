using System;
using System.Collections.Generic;

namespace KoiCoi.Database.AppDbContextModels;

public partial class Notification
{
    public int NotificationId { get; set; }

    public int UserId { get; set; }

    public int SenderId { get; set; }

    public string Title { get; set; } = null!;

    public string? Message { get; set; }

    public string Url { get; set; } = null!;

    public bool? IsRead { get; set; }

    public DateTime DateCreated { get; set; }
}
