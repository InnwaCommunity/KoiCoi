namespace KoiCoi.Models.EventDto;

public partial class CreateEventPayload
{
    public string? EventName { get; set; }
    public string? EventDescription { get; set; }
    public string? ChannelIdval { get; set; }
    public decimal? TargetBalance { get; set; }
    public string? StartDate { get; set; }
    public string? EndDate { get; set; }
    public List<EventAddressPayload> EventAddresses { get; set; }
    public List<EventPhotoPayload> EventPhotos { get; set; }
}

public partial class EventAddressPayload
{
    public string AddressTypeIdval { get; set; } = string.Empty;
    public string AddressName { get; set; } = string.Empty;
}