
namespace KoiCoi.Modules.Repository.PostFeature;

public  class BL_Post
{
    private readonly DA_Post _daPost;

    public BL_Post(DA_Post daPost)
    {
        _daPost = daPost;
    }

    public async Task<Result<string>> CreatePostFeature(CreatePostPayload payload,int LoginUserId)
    {
        return await _daPost.CreatePostFeature(payload, LoginUserId);
    }

    public async Task<Result<string>> CreatePostTags(CreatePostTagListPayload payload,int LoginUserId)
    {
        return await _daPost.CreatePostTags(payload, LoginUserId);
    }

    public async Task<Result<List<PostTagDataResponse>>> GetPostTags(string EventIdval,int LoginUserId)
    {
        return await _daPost.GetPostTags(EventIdval, LoginUserId);
    }
}
