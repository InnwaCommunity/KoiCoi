namespace KoiCoi.Models.EventDto.Response;

public partial class AllowedMarkResponse
{
    public string AllowedMarkIdval { get; set; } = string.Empty;
    public string MarkIdval { get; set; } = string.Empty;
    public string IsoCode { get; set; } = string.Empty;
    public string AllowedMarkName { get; set; } = string.Empty;
    public List<ExchangeRateResponse> ExchangeRates { get; set; } = new List<ExchangeRateResponse>();
}

public partial class ExchangeRateResponse
{
    public string ToMarkIdval { get; set; } = string.Empty;
    public string MarkName { get; set; } = string.Empty;
    public string IsoCode { get; set; } = string.Empty;
    public decimal MinQuantiry { get; set; } 
    public decimal Rate { get; set; }
}
