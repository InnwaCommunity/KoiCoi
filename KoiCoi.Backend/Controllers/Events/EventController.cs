using Microsoft.AspNetCore.Http.HttpResults;

namespace KoiCoi.Backend.Controllers.Events;

[Route("api/[controller]")]
[ApiController]
public class EventController : BaseController
{
    private readonly IConfiguration _configuration;
    private readonly BL_Event _blEvent;

    public EventController(BL_Event blEvent, IConfiguration configuration)
    {
        _blEvent = blEvent;
        _configuration = configuration;
    }

    [HttpPost("CreateEvent",Name = "CreateEvent")]
    public IActionResult CreateEvent(CreateEventPayload paylod)
    {
        return Ok(paylod);
    }
}
