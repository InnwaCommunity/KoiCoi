
using KoiCoi.Models.PostTagDto.Payload;
using KoiCoi.Models.PostTagDto.Response;

namespace KoiCoi.Backend.Controllers.PostFeature;

[Route("api/[controller]")]
[ApiController]
public class EventTagController : BaseController
{
    private readonly BL_Post _blPost;

    public EventTagController(BL_Post blPost)
    {
        _blPost = blPost;
    }

    [HttpPost("CreateEventTags", Name = "CreateEventTags")]
    public async Task<Result<string>> CreateEventTags(CreateEventTagListPayload payload)
    {
        int LoginUserID = Convert.ToInt32(_tokenData.LoginUserId);
        return await _blPost.CreateEventTags(payload, LoginUserID);
    }
    [HttpPost("GetEventTags", Name = "GetEventTags")]
    public async Task<Result<List<PostTagDataResponse>>> GetEventTags(GetEventDataPayload payload)
    {
        int LoginUserID = Convert.ToInt32(_tokenData.LoginUserId);
        return await _blPost.GetEventTags(payload.EventPostIdval!, LoginUserID);
    }

}
