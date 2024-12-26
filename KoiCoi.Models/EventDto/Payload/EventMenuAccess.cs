namespace KoiCoi.Models.EventDto.Payload;

public class EventMenuAccess
{
    public bool CanPostReview { get; set; } = false;
    public bool CanPostAction { get; set; } = false;
    public bool CanEditMember { get; set; } = false;
}
