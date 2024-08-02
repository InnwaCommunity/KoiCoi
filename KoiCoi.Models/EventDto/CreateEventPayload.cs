namespace KoiCoi.Models.EventDto;

public partial class CreateEventPayload
{
    public string? EventName { get; set; }
    public string? EventDescription { get; set; }
    public string? ChannelIdval { get; set; }
    public string? StartDate { get; set; }
    public string? EndDate { get; set; }
    public List<EventPhotoPayload> EventPhotos { get; set; }
}