

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
        int LoginUserId = Convert.ToInt32(_tokenData.LoginUserId);
        Result<string> response= await _blEvent.CreateEvent(paylod, LoginUserId);
        return Ok(response);
    }
    [HttpPost("UploadEventAttachFile",Name = "UploadEventAttachFile")]
    public async Task<Result<string>> UploadEventAttachFile(EventPhotoPayload payload)
    {
        int LoginUserId = Convert.ToInt32(_tokenData.LoginUserId);
        return await _blEvent.UploadEventAttachFile(payload, LoginUserId);
    }

    [HttpPost("GetEventRequestList",Name = "GetEventRequestList")]
    public async Task<IActionResult> GetEventRequestList(GetEventRequestPayload payload)
    {
        int LoginUserId = Convert.ToInt32(_tokenData.LoginUserId);
        Result<List<GetRequestEventResponse>> response = await _blEvent.GetEventRequestList(payload,LoginUserId);
        return Ok(response);
    }

    [HttpPost("ApproveRejectEvent",Name = "ApproveRejectEvent")]
    public async Task<IActionResult> ApproveRejectEvent(List<ApproveRejectEventPayload> payload)
    {
        int LoginUserId = Convert.ToInt32(_tokenData.LoginUserId);
        Result<string> response = await _blEvent.ApproveRejectEvent(payload,LoginUserId);
        return Ok(response);
    }

    [HttpPost("ChangeUserTypeTheEventMemberships",Name = "ChangeUserTypeTheEventMemberships")]
    public async Task<Result<string>> ChangeUserTypeTheEventMemberships(ChangeUserTypeEventMembership payload)
    {
        int LoginUserID = Convert.ToInt32(_tokenData.LoginUserId);
        return await _blEvent.ChangeUserTypeTheEventMemberships(payload, LoginUserID);
    }

    ///GET Event Owner and admins
    [HttpPost("GetEventOwnerAndAdmins",Name = "GetEventOwnerAndAdmins")]
    public async Task<Result<List<EventAdminsResponse>>> GetEventOwnerAndAdmins(GetEventDataPayload payload)
    {
        int LoginUserID = Convert.ToInt32(_tokenData.LoginUserId);
        return await _blEvent.GetEventOwnerAndAdmins(payload, LoginUserID);
    }

    [HttpGet("GetAddressTypes/{PageNumber}/{PageSize}",Name = "GetAddressTypes")]
    public async Task<Result<Pagination>> GetAddressTypes(int PageNumber,int PageSize)
    {
        int LoginUserID = Convert.ToInt32(_tokenData.LoginUserId);
        return await _blEvent.GetAddressTypes(LoginUserID,PageNumber,PageSize);
    }
    [HttpPost("EditStartDateandEndDate",Name = "EditStartDateandEndDate")]
    public async Task<Result<string>> EditStartDateandEndDate(EditStardEndDate payload)
    {
        int LoginUserID = Convert.ToInt32(_tokenData.LoginUserId);
        return await _blEvent.EditStartDateandEndDate(payload,LoginUserID);
    }
    [HttpPost("GetEventByStatusAndDate", Name = "GetEventByStatusAndDate")]
    public async Task<IActionResult> GetEventByStatusAndDate(OrderByMonthPayload payload)
    {
        int LoginUserId = Convert.ToInt32(_tokenData.LoginUserId);
        Result<Pagination> response = await _blEvent.GetEventByStatusAndDate(payload, LoginUserId);
        return Ok(response);
    }
}
