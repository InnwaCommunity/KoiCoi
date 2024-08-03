
namespace KoiCoi.Models.EventDto.Response;
public partial class GetRequestEventResponse
{
   public string? EventIdval { get; set; }
   public string? EventName { get; set; }
   public string? EventDescrition { get; set; }
   public string? CreatorIdval { get; set; }
   public string? CreatorName { get; set; }
   public string? StartDate { get; set; }
   public string? EndDate { get; set; }
   public string? ModifiedDate { get; set; }
   public List<EventImageInfo>? EventImageList { get; set; }
}

public partial class EventImageInfo
{
    public string? imgfilename { get; set; }
    public string? imgDescription { get; set; }
}


