using KoiCoi.Models.EventDto.Response;

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
    public async Task<IActionResult> CreateEvent(CreateEventPayload paylod)
    {
        int LoginUserId = Convert.ToInt32(_tokenData.LoginEmpID);
        Result<string> response= await _blEvent.CreateEvent(paylod, LoginUserId);
        return Ok(response);
    }

    [HttpPost("GetEventRequestList",Name = "GetEventRequestList")]
    public async Task<IActionResult> GetEventRequestList(GetEventRequestPayload payload)
    {
        int LoginUserId = Convert.ToInt32(_tokenData.LoginEmpID);
        Result<List<GetRequestEventResponse>> response = await _blEvent.GetEventRequestList(payload,LoginUserId);
        return Ok(response);
    }

    [HttpPost("ApproveRejectEvent",Name = "ApproveRejectEvent")]
    public async Task<IActionResult> ApproveRejectEvent(List<ApproveRejectEventPayload> payload)
    {
        int LoginUserId = Convert.ToInt32(_tokenData.LoginEmpID);
        Result<string> response = await _blEvent.ApproveRejectEvent(payload,LoginUserId);
        return Ok(response);
    }
}
