
using KoiCoi.Models.PostTagDto.Payload;
using KoiCoi.Models.PostTagDto.Response;

namespace KoiCoi.Backend.Controllers.PostFeature;

[Route("api/[controller]")]
[ApiController]
public class PostTagController : BaseController
{
    private readonly BL_Post _blPost;

    public PostTagController(BL_Post blPost)
    {
        _blPost = blPost;
    }

    [HttpPost("CreatePostTags", Name = "CreatePostTags")]
    public async Task<Result<string>> CreatePostTags(CreatePostTagListPayload payload)
    {
        int LoginUserID = Convert.ToInt32(_tokenData.LoginUserId);
        return await _blPost.CreatePostTags(payload, LoginUserID);
    }
    [HttpPost("GetPostTags", Name = "GetPostTags")]
    public async Task<Result<List<PostTagDataResponse>>> GetPostTags(GetEventDataPayload payload)
    {
        int LoginUserID = Convert.ToInt32(_tokenData.LoginUserId);
        return await _blPost.GetPostTags(payload.EventPostIdval!, LoginUserID);
    }

}
