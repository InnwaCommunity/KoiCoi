

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
    public async Task<Result<string>> CreateChannelType(ViaChannelType channelType)
    {
        return await _blChannel.CreateChannelType(channelType);
    }

    [HttpPost("UpdateChannelType", Name = "UpdateChannelType")]
    public async Task<Result<string>> UpdateChannelType(ChannelTypePayloads channelType)
    {
        try
        {
            int LoginEmpID = Convert.ToInt32(_tokenData.LoginUserId);
            int ChannelTypeId = Convert.ToInt32(Encryption.DecryptID(channelType.ChannelTypeIdval!, LoginEmpID.ToString()));
            return await _blChannel.UpdateChannelType(channelType,ChannelTypeId);
        }
        catch (Exception ex)
        {
            return Result<string>.Error(ex);
        }
    }

    [HttpDelete("DeleteChannelType/{id}",Name = "DeleteChannelType")]
    public async Task<Result<string>> DeleteChannelType(string id)
    {
        try
        {
            int LoginEmpID = Convert.ToInt32(_tokenData.LoginUserId);
            int ChannelTypeId = Convert.ToInt32(Encryption.DecryptID(id, LoginEmpID.ToString()));
            return await _blChannel.DeleteChannelType(ChannelTypeId);
        }
        catch (Exception ex)
        {
            return Result<string>.Error(ex);
        }
    }

    [HttpGet("GetChannelType",Name = "GetChannelType")]
    public async Task<Result<List<ChannelTypeResponseDto>>> GetChannelType()
    {
        int LoginEmpID = Convert.ToInt32(_tokenData.LoginUserId);
        return await _blChannel.GetChannelType(LoginEmpID);
    }
}
