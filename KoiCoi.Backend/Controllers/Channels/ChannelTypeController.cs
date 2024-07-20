

using KoiCoi.Database.AppDbContextModels;
using KoiCoi.Models.Via;

namespace KoiCoi.Backend.Controllers.Channels;

[Route("api/[controller]")]
[ApiController]
public class ChannelTypeController : BaseController
{
    private readonly BL_Channel _blChannel;

    public ChannelTypeController(BL_Channel blChannel)
    {
        _blChannel = blChannel;
    }

    [HttpPost("CreateChannelType",Name = "CreateChannelType")]
    public async Task<ResponseData> CreateChannelType(ViaChannelType channelType)
    {
        return await _blChannel.CreateChannelType(channelType);
    }

    [HttpPost("UpdateChannelType", Name = "UpdateChannelType")]
    public async Task<ResponseData> UpdateChannelType(ChannelTypePayloads channelType)
    {
        try
        {
            int LoginEmpID = Convert.ToInt32(_tokenData.LoginEmpID);
            int ChannelTypeId = Convert.ToInt32(Encryption.DecryptID(channelType.ChannelTypeIdval!, LoginEmpID.ToString()));
            return await _blChannel.UpdateChannelType(channelType,ChannelTypeId);
        }
        catch (Exception ex)
        {
            Console.WriteLine("GetApproverSettingWeb" + DateTime.Now + ex.Message);
            ResponseData data = new ResponseData();
            data.StatusCode = 0;
            data.Message = ex.Message;
            return data;
        }
    }

    [HttpDelete("DeleteChannelType/{id}",Name = "DeleteChannelType")]
    public async Task<ResponseData> DeleteChannelType(string id)
    {
        try
        {
            int LoginEmpID = Convert.ToInt32(_tokenData.LoginEmpID);
            int ChannelTypeId = Convert.ToInt32(Encryption.DecryptID(id, LoginEmpID.ToString()));
            return await _blChannel.DeleteChannelType(ChannelTypeId);
        }
        catch (Exception ex)
        {
            Console.WriteLine("GetApproverSettingWeb" + DateTime.Now + ex.Message);
            ResponseData data = new ResponseData();
            data.StatusCode = 0;
            data.Message = ex.Message;
            return data;
        }
    }

    [HttpGet("GetChannelType",Name = "GetChannelType")]
    public async Task<ResponseData> GetChannelType()
    {
        int LoginEmpID = Convert.ToInt32(_tokenData.LoginEmpID);
        return await _blChannel.GetChannelType(LoginEmpID);
    }
}
