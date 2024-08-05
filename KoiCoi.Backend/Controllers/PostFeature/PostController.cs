
namespace KoiCoi.Backend.Controllers.PostFeature;

[Route("api/[controller]")]
[ApiController]
public class PostController : BaseController
{
    private readonly IConfiguration _configuration;
    private readonly BL_Post _blPost;

    public PostController(BL_Post blPost, IConfiguration configuration)
    {
        _blPost = blPost;
        _configuration = configuration;
    }

    [HttpPost("CreatePostFeature",Name = "CreatePostFeature")]
    public async Task<Result<string>> CreatePostFeature(CreatePostPayload payload)
    {
        int LoginUserID = Convert.ToInt32(_tokenData.LoginUserId);
        return await _blPost.CreatePostFeature(payload, LoginUserID);
    }

    //[HttpPost("ReviewPosts",Name ="ReviewPosts")]
    //public async Task<Result<>> ReviewPosts()

    [HttpGet("GetPostPrivicy",Name = "GetPostPrivicy")]
    public async Task<Result<List<PostPrivacyResponse>>> GetPostPrivicy()
    {
        int LoginUserID = Convert.ToInt32(_tokenData.LoginUserId);
        return await _blPost.GetPostPrivicy(LoginUserID);
    }


    ///CreatePost
    ///ReviewPost
    ///ApproveAndRejectPost
    ///
}
