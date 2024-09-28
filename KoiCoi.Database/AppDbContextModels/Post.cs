using System;
using System.Collections.Generic;

namespace KoiCoi.Database.AppDbContextModels;

public partial class Post
{
    public int PostId { get; set; }

    public string PostType { get; set; } = null!;

    public DateTime ModifiedDate { get; set; }

    public DateTime CreatedDate { get; set; }

    public bool Inactive { get; set; }

    public virtual ICollection<ExchangeRate> ExchangeRates { get; set; } = new List<ExchangeRate>();
}
