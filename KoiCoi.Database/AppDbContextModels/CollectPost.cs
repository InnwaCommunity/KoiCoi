﻿using System;
using System.Collections.Generic;

namespace KoiCoi.Database.AppDbContextModels;

public partial class CollectPost
{
    public int CollectId { get; set; }

    public int PostId { get; set; }

    public string? Content { get; set; }

    public int EventPostId { get; set; }

    public int CreatorId { get; set; }

    public int? ApproverId { get; set; }

    public int StatusId { get; set; }
}
