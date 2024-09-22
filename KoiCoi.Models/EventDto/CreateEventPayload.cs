using KoiCoi.Models.PostDtos.Payload;

namespace KoiCoi.Models.EventDto;

public partial class CreateEventPayload
{
    public string? EventName { get; set; }
    public string? EventDescription { get; set; }
    public string? ChannelIdval { get; set; }
    public string? StartDate { get; set; }
    public string? EndDate { get; set; }
    public List<EventAddressPayload> EventAddresses { get; set; } = new List<EventAddressPayload>();
    public PostPolicyPropertyPayload viewPolicy { get; set; } = new PostPolicyPropertyPayload();
    public PostPolicyPropertyPayload reactPolicy { get; set; } = new PostPolicyPropertyPayload();
    public PostPolicyPropertyPayload commandPolicy { get; set; } = new PostPolicyPropertyPayload();
    public PostPolicyPropertyPayload sharePolicy { get; set; } = new PostPolicyPropertyPayload();
}

public partial class EventAddressPayload
{
    public string AddressTypeIdval { get; set; } = string.Empty;
    public string AddressName { get; set; } = string.Empty;
}