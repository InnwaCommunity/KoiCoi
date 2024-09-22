
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace KoiCoi.Models.EventDto.Response;
public partial class GetRequestEventResponse
{
   public string? EventPostIdval { get; set; }
   public string? EventName { get; set; }
   public string? EventDescrition { get; set; }
   public string? CreatorIdval { get; set; }
   public string? CreatorName { get; set; }
   //public decimal? TotalBalance { get; set; }
   public string? StartDate { get; set; }
   public string? EndDate { get; set; }
   public string? ModifiedDate { get; set; }
    public List<EventMarks>? EventMarks { get; set; }
    public List<EventAddressResponse>? AddressResponse { get; set; } 
    public List<EventFileInfo>? EventImageList { get; set; }
}
public partial class EventMarks
{
    public string? MarkIdval { get; set; }
    public string? IsoCode { get; set; }
    public string? MarkName { get; set; }
    public string? AllowedMarkName { get; set; }

    public decimal? TotalBalance { get; set; }
    public decimal? LastBalance { get; set; }
    public decimal? TargetBalance { get; set; }
}
public partial class EventFileInfo
{
    public string? fileIdval { get; set; }
    public string? imgfilename { get; set; }
    public string? imgDescription { get; set; }
}


public partial class EventAddressResponse
{
    public string? EventAddressIdval { get; set; }
    public string? AddresstypeName { get; set; }
    public string? Address { get; set; }
}


