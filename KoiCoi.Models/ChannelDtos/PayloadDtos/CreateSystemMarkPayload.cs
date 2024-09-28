namespace KoiCoi.Models.ChannelDtos.PayloadDtos;
public partial class CreateSystemMarkPayload
{
    public string MarkName { get; set; } = null!;

    public string MarkSymbol { get; set; } = null!;

    public string Isocode { get; set; } = null!;

    public string MarkTypeIdval { get; set; } = null!;

}
