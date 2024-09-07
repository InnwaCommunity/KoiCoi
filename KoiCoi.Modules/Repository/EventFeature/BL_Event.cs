
using KoiCoi.Models.EventDto.Payload;
using KoiCoi.Models.Login_Models;
using Microsoft.AspNetCore.Mvc;
using MimeKit.Tnef;

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
    public async Task<Result<string>> UploadEventAttachFile(EventPhotoPayload payload,int LoginUserId)
    {
        return await _daEvent.UploadEventAttachFile(payload, LoginUserId);
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
    public async Task<Result<Pagination>> GetAddressTypes(int LoginUserID,int PageNumber,int PageSize)
    {
        return await _daEvent.GetAddressTypes(LoginUserID,PageNumber,PageSize);
    }
    public async Task<Result<string>> EditStartDateandEndDate(EditStardEndDate payload, int LoginUserID )
    {
        return await _daEvent.EditStartDateandEndDate(payload, LoginUserID);
    }
    public async Task<Result<Pagination>> GetEventByStatusAndDate(OrderByMonthPayload payload,int LoginUserId)
    {
        return await _daEvent.GetEventByStatusAndDate(payload, LoginUserId);
    }

}
