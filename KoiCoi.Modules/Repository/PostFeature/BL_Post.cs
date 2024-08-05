
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
    public async Task<Result<List<PostPrivacyResponse>>> GetPostPrivicy(int LoginUserId)
    {
        return await _daPost.GetPostPrivicy(LoginUserId);
    }
}
