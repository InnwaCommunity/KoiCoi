
using KoiCoi.Models.EventDto.Payload;
using KoiCoi.Models.Login_Models;
using Microsoft.AspNetCore.Http;
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
    public async Task<Result<string>> UploadEventAttachFile(IFormFile file,string eventPostIdval, int LoginUserId)
    {
        return await _daEvent.UploadEventAttachFile(file, eventPostIdval, LoginUserId);
    }

    public async Task<Result<Pagination>> GetEventRequestList(GetEventRequestPayload payload,int LoginUserId)
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
    public async Task<Result<string>> CreateAllowedMarks(CreateAllowedMarkPayload payload,int LoginUserId)
    {
        return await _daEvent.CreateAllowedMarks(payload, LoginUserId);
    }
    public async Task<Result<string>> UpdateAllowdedMark(UpdateAllowdMarkPayload payload, int LoginUserId)
    {
        return await _daEvent.UpdateAllowdedMark(payload, LoginUserId);
    }

    public async Task<Result<Pagination>> GetAllowedMarks(GetAllowedMarkPayload payload,int LoginUserId)
    {
        return await _daEvent.GetAllowedMarks(payload, LoginUserId);
    }

    public async Task<Result<Pagination>> GetEventSupervisors(GetEventData payload,int LoginUserId)
    {
        return await _daEvent.GetEventSupervisors(payload, LoginUserId);
    }

    public async Task<Result<EventMenuAccess>> CheckEventAccessMenu(GetEventDataPayload payload,int LoginUserId)
    {
        return await _daEvent.CheckEventAccessMenu(payload, LoginUserId);
    }

    public async Task<Result<Pagination>> FindAccessEventByName(FindByNamePayload payload, int LoginUserId)
    {
        return await _daEvent.FindAccessEventByName(payload, LoginUserId);
    }
    public async Task<Result<Pagination>> EventContributionFilterMarkId(EventContributionPayload payload,int LoginUserId)
    {
        return await _daEvent.EventContributionFilterMarkId(payload, LoginUserId);
    }
    public async Task<Result<Pagination>> GetUserContributons(GetUserContributonsPayload payload,int LoginUserId)
    {
        return await _daEvent.GetUserContributons(payload, LoginUserId);
    }
    public async Task<Result<Pagination>> EventOverallContributions(GetEventData payload, int LoginUserId)
    {
        return await _daEvent.EventOverallContributions(payload, LoginUserId);
    }

}
