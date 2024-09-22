namespace KoiCoi.Models.PostDtos.Response;

public partial class ReviewPostResponse
{
    public string PostIdval { get; set; } = string.Empty;
    public string Content { get; set;} = string.Empty;
    public string? TagIdval { get; set; }
    public string? TagName { get; set; }
    public string CreatorIdval { get; set;} = string.Empty;
    public string CreatorName { get; set;} = string.Empty;
    public string? CreatorImageUrl { get; set; }
    public string EventIdval { get; set; }= string.Empty;
    public string EventName { get; set; } = string.Empty;
    public decimal CollectAmount { get; set; } = 0;
    public decimal? EventTotalAmount { get; set; }
    public string IsoCode { get; set; } = string.Empty;
    public string AllowedMarkName { get; set; } = string.Empty;
    public DateTime? CreatedDate { get; set; } 
    public List<PostImageResponse>? ImageResponse { get; set; }
}

public partial class PostImageResponse
{
    public string ImageIdval { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string? Description { get; set; }
}