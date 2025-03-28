﻿
using KoiCoi.Models.ChannelDtos.PayloadDtos;
using KoiCoi.Models.EventDto.Payload;
using KoiCoi.Models.Login_Models;
using KoiCoi.Modules.Repository.EventFreture;
using Microsoft.AspNetCore.Http;

namespace KoiCoi.Modules.Repository.ChannelFeature;

public class BL_Channel
{
    private readonly DA_Channel _daChannel;

    public BL_Channel(DA_Channel daChannel)
    {
        _daChannel = daChannel;
    }

    public async Task<Result<string>> CreateChannelType(ViaChannelType channelTypePayloads)
    {
        return await _daChannel.CreateChannelType(channelTypePayloads);
    }

    public async Task<Result<string>> UpdateChannelType(ChannelTypePayloads channelType,int ChannelTypeid)
    {
        return await _daChannel.UpdateChannelType(channelType, ChannelTypeid);
    }

    public async Task<Result<string>> DeleteChannelType(int ChannelTypeid)
    {
        return await _daChannel.DeleteChannelType(ChannelTypeid);
    }

    public async Task<Result<List<ChannelTypeResponseDto>>> GetChannelType(int loginUserid)
    {
        return await _daChannel.GetChannelType(loginUserid);
    }

    public async Task<Result<string>> CreateCustomMark(CreateCustomMarkPayload payload,int LoginUserID)
    {
        return await _daChannel.CreateCustomMark(payload, LoginUserID);
    }

    public async Task<Result<Pagination>> GetMarkList(int LoginUserId,GetMarkPayload payload)
    {
        return await _daChannel.GetMarkList(LoginUserId,payload);
    }

    public async Task<Result<Pagination>> GetMarkType(int LoginUserId,int PageNumber, int PageSize)
    {
        return await _daChannel.GetMarkType(LoginUserId,PageNumber,PageSize);
    }

    public async Task<Result<string>> CreateChannel(CreateChannelReqeust channelReqeust ,int LoginUserId)
    {
        return await _daChannel.CreateChannel(channelReqeust, LoginUserId);
    }

    public async Task<Result<Pagination>> GetChannelsList(int LoginUserId,int PageNumber,int PageSize, string Status)
    {
        return await _daChannel.GetChannelsList(LoginUserId,PageNumber,PageSize,Status);
    }
    /*public async Task<Result<string>> GetChannelProfile(int ChannelId,string destDir)
    {
        return await _daChannel.GetChannelProfile(ChannelId,destDir);
    }*/

    public async Task<Result<string>> UploadChannelProfile(IFormFile file, string ChannelIdval, int LoginUserId)
    {
        return await _daChannel.UploadChannelProfile(file, ChannelIdval, LoginUserId);
    }

    /*public async Task<Result<string>> UploadProfile(int LoginUserId,int ChannelId,string filename,string? imgDes)
    {
        return await _daChannel.UploadProfile(LoginUserId,ChannelId,filename,imgDes);
    }*/

    public async Task<Result<string>> GenerateChannelUrl(int ChannelId,int LoginUserId)
    {
        return await _daChannel.GenerateChannelUrl(ChannelId,LoginUserId); 
    }

    public async Task<Result<VisitChannelResponse>> VisitChannelByInviteLink(string inviteLink,int LoginUserId)
    {
        return await _daChannel.VisitChannelByInviteLink(inviteLink,LoginUserId);
    }

    public async Task<Result<string>> JoinChannelByInviteLink(JoinChannelInviteLinkPayload payload,int LoginUserId)
    {
        return await _daChannel.JoinChannelByInviteLink(payload, LoginUserId);
    }

    public async Task<Result<List<ChannelMemberResponse>>> GetChannelMember(string ChannelIdval,string MemberStatus,int LoginUserId)
    {
        return await _daChannel.GetChannelMember(ChannelIdval,MemberStatus, LoginUserId);
    }

    public async Task<Result<string>> ApproveRejectChannelMember(List<AppRejChannelMemberPayload> payload,int LoginUserId)
    {
        return await _daChannel.ApproveRejectChannelMember(payload,LoginUserId);
    }
    public async Task<Result<List<VisitUserResponse>>> GetVisitUsersRecords(GetVisitUsersPayload payload,int LoginUserId)
    {
        return await _daChannel.GetVisitUsersRecords(payload,LoginUserId);
    }
    public async Task<Result<List<VisitUserResponse>>> NewMembersRecords(GetVisitUsersPayload payload, int LoginUserId)
    {
        return await _daChannel.NewMembersRecords(payload, LoginUserId);
    }

    public async Task<Result<string>> LeaveChannel(string channelIdval,int LoginUserId)
    {
        return await _daChannel.LeaveChannel(channelIdval, LoginUserId);
    }

    public async Task<Result<string>> RemoveMemberByAdmin(string channelIdval,List<RemoveMemberData> members,int LoginUserId)
    {
        return await _daChannel.RemoveMemberByAdmin(channelIdval,members,LoginUserId);
    }

    public async Task<Result<string>> ChangeUserTypeTheChannelMemberships(ChangeUserTypeChannelMembership payload,int LoginUserId)
    {
        return await _daChannel.ChangeUserTypeTheChannelMemberships(payload,LoginUserId);
    }
    public async Task<Result<ChannelAccessMenu>> CheckChannelAccessMenu(GetChannelData payload,int LoginUserID)
    {
        return await _daChannel.CheckChannelAccessMenu(payload, LoginUserID);
    }

    public async Task<Result<Pagination>> FindAccessChannelByName(FindByNamePayload payload,int LoginUserID)
    {
        return await _daChannel.FindAccessChannelByName(payload, LoginUserID);
    }
    public async Task<Result<Pagination>> ChannelOverallContribution(OverallContributionPayload payload,int LoginUserID)
    {
        return await _daChannel.ChannelOverallContribution(payload, LoginUserID);
    }
}
