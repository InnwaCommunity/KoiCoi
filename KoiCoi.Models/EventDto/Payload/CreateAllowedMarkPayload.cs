
namespace KoiCoi.Models.EventDto.Payload;

public class CreateAllowedMarkPayload
{
    public string EventIdval { get; set; } = string.Empty;
    public List<AllowMarkPayload> AllowMarkPayloads { get; set; }  = new List<AllowMarkPayload>();
}

public class AllowMarkPayload
{
    public string MarkIdval { get; set; } = string.Empty;
    public string MarkName { get; set;} = string.Empty;
    public decimal? TargetBalance { get; set; }
    public List<ExchangeRatePayload> ExchangeRatePayloads { get; set; } = new List<ExchangeRatePayload> { };
}

public partial class ExchangeRatePayload
{
    public string ToMarkIdval { get; set; }= string.Empty;
    public decimal MinQuantity { get; set; }
    public decimal Rate { get; set; }
}
