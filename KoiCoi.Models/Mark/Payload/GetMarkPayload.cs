namespace KoiCoi.Models.Mark.Payload;

public partial class GetMarkPayload
{
    public string MarkTypeIdval { get; set; } = string.Empty;
    public int pageNumber { get; set; }
    public int pageSize { get; set; }
}
