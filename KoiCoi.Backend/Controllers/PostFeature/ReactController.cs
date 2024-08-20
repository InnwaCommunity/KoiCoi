namespace KoiCoi.Backend.Controllers.PostFeature;

[Route("api/[controller]")]
[ApiController]
public class ReactController : BaseController
{
    private readonly BL_React _blReact;

    public ReactController(BL_React blReact)
    {
        _blReact = blReact;
    }

    [HttpGet("GetAllReactType",Name = "GetAllReactType")]
    public async Task<Result<List<ReactTypeResponse>>> GetAllReactType()
    {
        int LoginUserID = Convert.ToInt32(_tokenData.LoginUserId);
        return await _blReact.GetAllReactType(LoginUserID);
    }

    [HttpPost("ReactPost",Name = "ReactPost")]
    public async Task<Result<string>> ReactPost(ReactPostPayload payload)
    {
        int LoginUserID = Convert.ToInt32(_tokenData.LoginUserId);
        return await _blReact.ReactPost(payload,LoginUserID);
    }

    ///Like
    ///Command
    ///Share
    ///View
}
