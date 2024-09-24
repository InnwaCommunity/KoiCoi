namespace KoiCoi.Models.PostTagDto.Payload;



public partial class CreateEventTagListPayload
{
    public string? EventPostIdval { get; set; }
    public List<EventTagPayload> EventTags { get; set; } = new List<EventTagPayload>();

}

public partial class EventTagPayload
{
    public string EventTagName { get; set; } = string.Empty;
    public string EventTagDescritpion { get; set; } = string.Empty;
}

