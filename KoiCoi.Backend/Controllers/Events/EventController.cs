

using KoiCoi.Modules.Repository.PostFeature;

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
    public async Task<Result<string>> UploadEventAttachFile([FromForm] string eventPostIdval, [FromForm] IFormFile files)
    {
        try
        {
            if (files != null && files.Length > 0)
            {
                int LoginUserId = Convert.ToInt32(_tokenData.LoginUserId);
                return await _blEvent.UploadEventAttachFile(files, eventPostIdval, LoginUserId);
            }
            else
            {
                return Result<string>.Error("No file uploaded.");
            }
        }
        catch (Exception ex)
        {
            return Result<string>.Error(ex.Message);
        }
    }

    [HttpPost("GetEventRequestList",Name = "GetEventRequestList")]
    public async Task<IActionResult> GetEventRequestList(GetEventRequestPayload payload)
    {
        int LoginUserId = Convert.ToInt32(_tokenData.LoginUserId);
        Result<Pagination> response = await _blEvent.GetEventRequestList(payload,LoginUserId);
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

    [HttpPost("CreateAllowedMarks",Name = "CreateAllowedMarks")]
    public async Task<Result<string>> CreateAllowedMarks(CreateAllowedMarkPayload payload)
    {
        int LoginUserId = Convert.ToInt32(_tokenData.LoginUserId);
        return await _blEvent.CreateAllowedMarks(payload,LoginUserId);
    }
    [HttpPut("UpdateAllowdedMark",Name = "UpdateAllowdedMark")]
    public async Task<Result<string>> UpdateAllowdedMark(UpdateAllowdMarkPayload payload)
    {
        int LoginUserId = Convert.ToInt32(_tokenData.LoginUserId);
        return await _blEvent.UpdateAllowdedMark(payload, LoginUserId);
    }


    [HttpPost("GetAllowedMarks", Name = "GetAllowedMarks")]
    public async Task<Result<Pagination>> GetAllowedMarks(GetAllowedMarkPayload payload)
    {
        int LoginUserId = Convert.ToInt32(_tokenData.LoginUserId);
        return await _blEvent.GetAllowedMarks(payload, LoginUserId);
    }

    [HttpPost("GetEventSupervisors",Name = "GetEventSupervisors")]
    public async Task<Result<Pagination>> GetEventSupervisors(GetEventData payload)
    {
        int LoginUserId = Convert.ToInt32(_tokenData.LoginUserId);
        return await _blEvent.GetEventSupervisors(payload, LoginUserId);
    }
    [HttpPost("CheckEventAccessMenu",Name = "CheckEventAccessMenu")]
    public async Task<Result<EventMenuAccess>> CheckEventAccessMenu(GetEventDataPayload payload)
    {
        int LoginUserId = Convert.ToInt32(_tokenData.LoginUserId);
        return await _blEvent.CheckEventAccessMenu(payload, LoginUserId);
    }
    [HttpPost("FindAccessEventByName", Name = "FindAccessEventByName")]
    public async Task<Result<Pagination>> FindAccessEventByName(FindByNamePayload payload)
    {
        int LoginUserId = Convert.ToInt32(_tokenData.LoginUserId);
        return await _blEvent.FindAccessEventByName(payload, LoginUserId);
    }
    [HttpPost("EventContributionFilterMarkId", Name = "EventContributionFilterMarkId")]
    public async Task<Result<Pagination>> EventContributionFilterMarkId(EventContributionPayload payload)
    {
        int LoginUserId = Convert.ToInt32(_tokenData.LoginUserId);
        return await _blEvent.EventContributionFilterMarkId(payload, LoginUserId);
    }
    [HttpPost("GetUserContributons",Name = "GetUserContributons")]
    public async Task<Result<Pagination>> GetUserContributons(GetUserContributonsPayload payload)
    {
        int LoginUserId = Convert.ToInt32(_tokenData.LoginUserId);
        return await _blEvent.GetUserContributons(payload, LoginUserId);
    }
}
