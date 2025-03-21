﻿using System;
using System.Collections.Generic;

namespace KoiCoi.Database.AppDbContextModels;

public partial class Event
{
    public int Eventid { get; set; }

    public int PostId { get; set; }

    public string EventName { get; set; } = null!;

    public string? EventDescription { get; set; }

    public int ChannelId { get; set; }

    public int CreatorId { get; set; }

    public int? ApproverId { get; set; }

    public int StatusId { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }
}
