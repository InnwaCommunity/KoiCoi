namespace KoiCoi.Models.PostTagDto.Payload;



public partial class CreatePostTagListPayload
{
    public string? EventPostIdval { get; set; }
    public List<PostTagPayload> PostTags { get; set; } = new List<PostTagPayload>();

}

public partial class PostTagPayload
{
    public string PostTagName { get; set; } = string.Empty;
    public string PostTagDescritpion { get; set; } = string.Empty;
}

