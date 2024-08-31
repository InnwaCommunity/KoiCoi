namespace KoiCoi.Models.Currency;

public partial class MarkResponseDto
{
    public string? MarkIdval { get; set; }

    public string MarName { get; set; } = null!;

    public string MarkSymbol { get; set; } = null!;

    public string IsoCode { get; set; } = null!;
    public string TypeIdval { get; set; } = null!;
    public string TypeName { get; set; } = null!;
}
