namespace KoiCoi.Models.PostDtos.Payload;

public partial class CreatePostPayload
{
    public string? Content { get; set; }
    public string? EventIdval { get; set; }
    public string? TagIdval { get; set; }
    public decimal CollectAmount { get; set; }
    public List<PostImagePayload>? PostImageData { get; set; }
    public PostPolicyPropertyPayload? ViewPolicy { get; set; }
    public PostPolicyPropertyPayload? ReactPolicy { get; set; }
    public PostPolicyPropertyPayload? CommandPolicy { get; set; }
    public PostPolicyPropertyPayload? SharePolicy { get; set; }
}


public partial class PostPolicyPropertyPayload
{
    public int? MaxCount { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public bool? GroupMemberOnly { get; set; }

    public bool? FriendOnly { get; set; }
}
