

namespace KoiCoi.Models.PostDtos.Response;

public partial class DashboardPost
{
    public string PostIdval { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string ChannelIdval { get; set; } = string.Empty;
    public string ChannelName { get; set;} = string.Empty;
    public string EventIdval { get; set; } = string.Empty;
    public string EventName { get; set; } = string.Empty;
    public string? TagIdval { get; set; }
    public string? TagName { get; set; }
    public string CreatorIdval { get; set; } = string.Empty;
    public string CreatorName { get; set; } = string.Empty;
    public decimal CollectAmount { get; set; } = 0;
    public DateTime? ModifiedDate { get; set; }
    public DateTime? CreatedDate { get; set; }
    public int LikeTotalCount { get; set; }
    public int CommandCount { get; set; }
    public int ShareTotalCount { get; set; }
    public int ViewTotalCount { get; set; }
    public bool CanLike { get; set; }
    public bool CanCommand { get; set; }
    public bool CanShare { get; set; }
    public List<PostImageResponse>? ImageResponse { get; set; }
}


public partial class DashboardPostsResponse
{
    public int PageCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public List<DashboardPost> dashboardPost { get; set; } = new List<DashboardPost>();
}