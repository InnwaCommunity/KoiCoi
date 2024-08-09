using KoiCoi.Models.Login_Models;

namespace KoiCoi.Backend.Controllers.PostFeature;

[Route("api/[controller]")]
[ApiController]
public class ReactController : BaseController
{
    private readonly BL_Post _blPost;

    public ReactController(BL_Post blPost)
    {
        _blPost = blPost;
    }

    [HttpGet("GetAllReactType",Name = "GetAllReactType")]
    public async Task<Result<List<ReactTypeResponse>>> GetAllReactType()
    {
        int LoginUserID = Convert.ToInt32(_tokenData.LoginUserId);
        return await _blPost.GetAllReactType(LoginUserID);
    }
}
