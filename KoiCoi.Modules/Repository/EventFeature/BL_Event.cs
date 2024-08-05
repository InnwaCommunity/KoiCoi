
namespace KoiCoi.Modules.Repository.EventFreture;

public class BL_Event
{
    private readonly DA_Event _daEvent;

    public BL_Event(DA_Event daEvent)
    {
        _daEvent = daEvent;
    }

    public async Task<Result<string>> CreateEvent(CreateEventPayload paylod,int LoginUserId)
    {
        return await _daEvent.CreateEvent(paylod, LoginUserId);
    }

    public async Task<Result<List<GetRequestEventResponse>>> GetEventRequestList(GetEventRequestPayload payload,int LoginUserId)
    {
        return await _daEvent.GetEventRequestList(payload, LoginUserId);
    }

    public async Task<Result<string>> ApproveRejectEvent(List<ApproveRejectEventPayload> payload,int LoginUserId)
    {
        return await _daEvent.ApproveRejectEvent(payload, LoginUserId);
    }

    public async Task<Result<string>> ChangeUserTypeTheEventMemberships(ChangeUserTypeEventMembership payload, int LoginUserId)
    {
        return await _daEvent.ChangeUserTypeTheEventMemberships(payload, LoginUserId);
    }

    public async Task<Result<List<EventAdminsResponse>>> GetEventOwnerAndAdmins(GetEventDataPayload payload,int LoginUserId)
    {
        return await _daEvent.GetEventOwnerAndAdmins(payload, LoginUserId);
    }
}
