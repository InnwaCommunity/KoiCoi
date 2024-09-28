namespace KoiCoi.Models.ChannelDtos.PayloadDtos;

public partial class CreateCustomMarkPayload
{
    public string ChannelIdval { get; set; } = null!;
    public List<MarkPayload> MarkPayloads { get; set; } = new List<MarkPayload>();
}

public partial class MarkPayload
{
    public string MarkName { get; set; } = null!;

    public string MarkSymbol { get; set; } = null!;

    public string Isocode { get; set; } = null!;

    public string MarkTypeIdval { get; set; } = null!;

    public string? UserIdval { get; set; }
}