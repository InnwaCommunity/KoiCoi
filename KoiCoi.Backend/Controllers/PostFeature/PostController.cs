using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace KoiCoi.Backend.Controllers.PostFeature;

[Route("api/[controller]")]
[ApiController]
public class PostController : BaseController
{
    private readonly IConfiguration _configuration;
    private readonly BL_Event _blEvent;

    public PostController(BL_Event blEvent, IConfiguration configuration)
    {
        _blEvent = blEvent;
        _configuration = configuration;
    }


    ///CreatePost
    ///ReviewPost
    ///ApproveAndRejectPost
    ///
}
