

namespace KoiCoi.Models.PostDtos.Response;

public partial class DashboardPostsResponse
{
    public string PostIdval { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string ChannelIdval { get; set; } = string.Empty;
    public string ChannelName { get; set;} = string.Empty;
    public string EventPostIdval { get; set; } = string.Empty;
    public string EventName { get; set; } = string.Empty;
    public string? TagIdval { get; set; }
    public string? TagName { get; set; }
    public string CreatorIdval { get; set; } = string.Empty;
    public string CreatorName { get; set; } = string.Empty;
    public decimal CollectAmount { get; set; } = 0;
    public decimal EventTotalAmount { get; set; } = 0;
    public string IsoCode { get; set; } = string.Empty;
    public string AllowedMarkName { get; set;} = string.Empty;
    public DateTime? ModifiedDate { get; set; }
    public DateTime? CreatedDate { get; set; }
    public int LikeTotalCount { get; set; }
    public int CommandTotalCount { get; set; }
    public int ShareTotalCount { get; set; }
    public int ViewTotalCount { get; set; }
    public bool CanLike { get; set; }
    public bool CanCommand { get; set; }
    public bool CanShare { get; set; }
    public List<PostImageResponse>? ImageResponse { get; set; }
}

public partial class PostReactStatus { 
    public string? reactIdval { get; set; }
    public string? reacttypeIdval { get; set; }
    public string? reactTypeName { get; set; }
}
