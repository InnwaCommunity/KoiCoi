using KoiCoi.Modules.Repository.EventFreture;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace KoiCoi.Backend.Controllers.Events;

[Route("api/[controller]")]
[ApiController]
public class EventController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly BL_Event _blEvent;

    public EventController(BL_Event blEvent, IConfiguration configuration)
    {
        _blEvent = blEvent;
        _configuration = configuration;
    }
}
