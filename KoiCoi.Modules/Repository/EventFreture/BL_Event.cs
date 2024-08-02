using KoiCoi.Models.EventDto;
using KoiCoi.Modules.Repository.Channel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
}
