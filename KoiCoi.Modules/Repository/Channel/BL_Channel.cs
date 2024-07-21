using KoiCoi.Models.Via;
using KoiCoi.Modules.Repository.ChangePassword;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KoiCoi.Modules.Repository.Channel;

public class BL_Channel
{
    private readonly DA_Channel _daChannel;

    public BL_Channel(DA_Channel daChannel)
    {
        _daChannel = daChannel;
    }

    public async Task<ResponseData> CreateChannelType(ViaChannelType channelTypePayloads)
    {
        return await _daChannel.CreateChannelType(channelTypePayloads);
    }

    public async Task<ResponseData> UpdateChannelType(ChannelTypePayloads channelType,int ChannelTypeid)
    {
        return await _daChannel.UpdateChannelType(channelType, ChannelTypeid);
    }

    public async Task<ResponseData> DeleteChannelType(int ChannelTypeid)
    {
        return await _daChannel.DeleteChannelType(ChannelTypeid);
    }

    public async Task<ResponseData> GetChannelType(int loginUserid)
    {
        return await _daChannel.GetChannelType(loginUserid);
    }

    
    
    public async Task<ResponseData> GetCurrencyList(int LoginUserId)
    {
        return await _daChannel.GetCurrencyList(LoginUserId);
    }

    public async Task<ResponseData> CreateChannel(CreateChannelReqeust channelReqeust ,int LoginUserId,string filename)
    {
        return await _daChannel.CreateChannel(channelReqeust, LoginUserId,filename);
    }

    public async Task<ResponseData> GetChannels(int LoginUserId)
    {
        return await _daChannel.GetChannels(LoginUserId);
    }

    public async Task<ResponseData> GetChannelProfile(int ChannelId,string destDir)
    {
        return await _daChannel.GetChannelProfile(ChannelId,destDir);
    }

    public async Task<ResponseData> UploadProfile(int LoginUserId,int ChannelId,string filename,string? imgDes)
    {
        return await _daChannel.UploadProfile(LoginUserId,ChannelId,filename,imgDes);
    }

    public async Task<ResponseData> GenerateChannelUrl(int ChannelId,int LoginUserId)
    {
        return await _daChannel.GenerateChannelUrl(ChannelId,LoginUserId); 
    }

    public async Task<ResponseData> VisitChannelByInviteLink(string inviteLink,int LoginUserId)
    {
        return await _daChannel.VisitChannelByInviteLink(inviteLink,LoginUserId);
    }
}
