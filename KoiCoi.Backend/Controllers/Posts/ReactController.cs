using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace KoiCoi.Backend.Controllers.Posts;

[Route("api/[controller]")]
[ApiController]
public class ReactController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly BL_Event _blEvent;

    public ReactController(BL_Event blEvent, IConfiguration configuration)
    {
        _blEvent = blEvent;
        _configuration = configuration;
    }
}
