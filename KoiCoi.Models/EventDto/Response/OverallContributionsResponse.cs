namespace KoiCoi.Models.EventDto.Response;

public partial class OverallContributionsResponse
{
    public string ContributorIdval { get; set; } = string.Empty;
    public string ContributorName { get; set; }= string.Empty;
    public string Contact { get; set; } = string.Empty;
    public List<ContributionResponse> contributions { get; set; } = new List<ContributionResponse>();
}

public partial class ContributionResponse
{
    public string MarkIdval { get; set; } = string.Empty;
    public string MarkName { get; set; } = string.Empty;
    public string IsoCode {  get; set; } = string.Empty;
    public decimal CollectBalance { get; set; } = 0;
    public decimal TotalBalance { get; set; } = 0;
}