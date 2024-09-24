namespace KoiCoi.Models.PostDtos.Response;

public partial class ReviewPostResponse
{
    public string PostIdval { get; set; } = string.Empty;
    public string Content { get; set;} = string.Empty;
    public string CreatorIdval { get; set;} = string.Empty;
    public string CreatorName { get; set;} = string.Empty;
    public string? CreatorImageUrl { get; set; }
    public string EventIdval { get; set; }= string.Empty;
    public string EventName { get; set; } = string.Empty;
    public DateTime? CreatedDate { get; set; }
    public List<PostTagResponse> postTagRes { get; set; } = new List<PostTagResponse>();
    public List<PostBalanceResponse> postBalanceRes { get; set; } = new List<PostBalanceResponse>();
    public List<PostImageResponse> ImageResponse { get; set; } = new List<PostImageResponse>();
}

public partial class PostImageResponse
{
    public string ImageIdval { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public partial class PostBalanceResponse
{
    public decimal CollectAmount { get; set; } = 0;
    public decimal? EventTotalAmount { get; set; }
    public string IsoCode { get; set; } = string.Empty;
    public string AllowedMarkName { get; set; } = string.Empty;
}

public partial class PostTagResponse
{
    public string? PostTagIdval { get; set; }
    public string? TagName { get; set; }
}