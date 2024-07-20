using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KoiCoi.Models.Currency;

public partial class CurrencyResponseDto
{
    public string? CurrencyIdval { get; set; }

    public string CurrencyName { get; set; } = null!;

    public string CurrencySymbol { get; set; } = null!;

    public string IsoCode { get; set; } = null!;

    public string FractionalUnit { get; set; } = null!;
}
