namespace KoiCoi.Models.PostDtos.Payload;

public partial class CreatePostPayload
{
    public string? Content { get; set; }
    public string? EventPostIdval { get; set; }
    public string? TagIdval { get; set; }
    public decimal CollectAmount { get; set; }
    public PostPolicyPropertyPayload viewPolicy { get; set; } = new PostPolicyPropertyPayload();
    public PostPolicyPropertyPayload reactPolicy { get; set; } = new PostPolicyPropertyPayload();
    public PostPolicyPropertyPayload commandPolicy { get; set; } = new PostPolicyPropertyPayload();
    public PostPolicyPropertyPayload sharePolicy { get; set; } = new PostPolicyPropertyPayload();
    //public List<PostPolicyPropertyPayload> policyProperties { get; set; } = new List<PostPolicyPropertyPayload>();
    //public List<PostImagePayload> imageData { get; set; } = new List<PostImagePayload>();
}


public partial class PostPolicyPropertyPayload
{
    public int? MaxCount { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public bool? GroupMemberOnly { get; set; }

    public bool? FriendOnly { get; set; }
}
