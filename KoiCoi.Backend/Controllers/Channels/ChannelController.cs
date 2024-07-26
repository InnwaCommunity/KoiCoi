using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QRCoder;
using Serilog;
using System.Configuration;
using System.Drawing.Imaging;
using System.Drawing;
using KoiCoi.Models.ChannelDtos.ResponseDtos;
using static QRCoder.PayloadGenerator;
using System.Collections.Generic;

namespace KoiCoi.Backend.Controllers.Channels;

[Route("api/[controller]")]
[ApiController]
public class ChannelController : BaseController
{
    private readonly IConfiguration _configuration;
    private readonly BL_Channel _blChannel;

    public ChannelController(BL_Channel blChannel, IConfiguration configuration)
    {
        _blChannel = blChannel;
        _configuration = configuration;
    }


    [HttpGet("GetCurrencyList", Name = "GetCurrencyList")]
    public async Task<Result<List<CurrencyResponseDto>>> GetCurrencyList()
    {
        int LoginEmpID = Convert.ToInt32(_tokenData.LoginEmpID);
        return await _blChannel.GetCurrencyList(LoginEmpID);
    }

    /*[HttpPost("UploadChannelProfileTemp", Name = "UploadChannelProfileTemp")]
    public async Task<ResponseData> UploadChannelProfileTemp()
    {
        Response.ContentType = "application/json";
        ResponseData responseData = new ResponseData();
        try
        {
            var files = Request.Form.Files;
            if (files.Count > 0)
            {
                // Save the file
                var file = files[0];
                if (file.Length > 0)
                {
                    string uploadfilename = file.FileName;
                    string ext = FileService.GetFileExtension(uploadfilename);

                    string fullPath = "";
                    string[] allowext = _configuration.GetSection("appSettings:AllowExtension").Get<string[]>()!;
                    string folderPath = _configuration["appSettings:UploadChannelProfilePath"] ?? throw new Exception("Invalid temp path.");
                    string baseDirectory = _configuration["appSettings:UploadPath"] ?? throw new Exception("Invalid UploadPath");
                    if (!allowext.Contains(ext))
                    {
                        throw new Exception("Invalid File Extension " + ext);
                    }
                    folderPath = baseDirectory + folderPath;//flodrer import
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    string filename = Guid.NewGuid().ToString() + "." + ext.ToLower();
                    fullPath = folderPath + filename ;

                    if (fullPath.Contains(".."))
                    { //if found .. in the file name or path
                        Log.Error("Invalid path " + fullPath);
                        throw new Exception("Invalid path");
                    }

                    using (var fileStream = new FileStream(fullPath, FileMode.OpenOrCreate))
                    {
                        await file.CopyToAsync(fileStream);
                    }
                    responseData.StatusCode = 1;
                    responseData.Message = "Upload Success";
                    responseData.Data = filename;
                    return responseData;
                }
                else

                    throw new Exception("Empty File.");
            }
            else
                throw new Exception("No File.");

        }
        catch (Exception ex)
        {
            responseData.StatusCode = 0;
            responseData.Message = ex.Message;
            return responseData;
        }
    }
     */

    [HttpPost("CreateChannel",Name = "CreateChannel")]
    public async Task<Result<ChannelDataResponse>> CreateChannel(CreateChannelReqeust channelReqeust)
    {
        try
        {
            string filename = "";
            if (!string.IsNullOrEmpty(channelReqeust.ProImage64))
            {
                string baseDirectory = _configuration["appSettings:UploadPath"] ?? throw new Exception("Invalid UploadPath");
                //string tempfolderPath = _configuration["appSettings:UploadChannelProfilePath"] ?? throw new Exception("Invalid temp path.");
                string uploadDirectory = _configuration["appSettings:ChannelProfile"] ?? throw new Exception("Invalid function upload path.");

                string folderPath = Path.Combine(baseDirectory, uploadDirectory);


                filename = Guid.NewGuid().ToString() + "." + ".png";
                string base64Str = channelReqeust.ProImage64;
                byte[] bytes = Convert.FromBase64String(base64Str!);

                string filePath = Path.Combine(folderPath, filename);
                if (filePath.Contains(".."))
                { //if found .. in the file name or path
                    Log.Error("Invalid path " + filePath);
                    throw new Exception("Invalid path");
                }
                await System.IO.File.WriteAllBytesAsync(filePath, bytes);
            }
                

                int LoginEmpID = Convert.ToInt32(_tokenData.LoginEmpID);
                return await _blChannel.CreateChannel(channelReqeust, LoginEmpID, filename);
        }
        catch (Exception ex)
        {
            return Result<ChannelDataResponse>.Error(ex);
        }
    }

    [HttpGet("GetChannelsList", Name = "GetChannelsList")]
    public async Task<Result<List<ChannelDataResponse>>> GetChannelsList()
    {

        int LoginEmpID = Convert.ToInt32(_tokenData.LoginEmpID);
        return await _blChannel.GetChannelsList(LoginEmpID);

    }

    [HttpPost("GetChannelProfile",Name = "GetChannelProfile")]
    public async Task<Result<string>> GetChannelProfile(GetChannelData getChannelData)
    {
        try
        {
            string? channelIdval = getChannelData.channelIdval;
            if (string.IsNullOrEmpty(channelIdval)) throw new Exception("Channel Id can't Null Or Empty");

            string baseDirectory = _configuration["appSettings:UploadPath"] ?? throw new Exception("Invalid UploadPath");
            string uploadDirectory = _configuration["appSettings:ChannelProfile"] ?? throw new Exception("Invalid function upload path.");
            string destinationDirectory = Path.Combine(baseDirectory, uploadDirectory);
            int LoginEmpID = Convert.ToInt32(_tokenData.LoginEmpID);
            int ChannelId = Convert.ToInt32(Encryption.DecryptID(channelIdval, LoginEmpID.ToString()));
            return await _blChannel.GetChannelProfile(ChannelId, destinationDirectory);
        }
        catch (Exception ex)
        {
            return Result<string>.Error(ex);
        }

    }
    [HttpPost("DirectUploadChannelProfile",Name = "DirectUploadChannelProfile")]

    public async Task<Result<string>> DirectUploadChannelProfile(UploadChannelProfileRequest updoadReqeust)
    {
        Response.ContentType = "application/json";
        try
        {
            if (string.IsNullOrEmpty(updoadReqeust.ChannelIdval)) throw new Exception("ChannelId can't be null or empty");
            int LoginEmpID = Convert.ToInt32(_tokenData.LoginEmpID);
            int ChannelId = Convert.ToInt32(Encryption.DecryptID(updoadReqeust.ChannelIdval, LoginEmpID.ToString()));

            if (!string.IsNullOrEmpty(updoadReqeust.base64data))
            {

                    string[] allowext = _configuration.GetSection("appSettings:AllowExtension").Get<string[]>()!;
                    string folderPath = _configuration["appSettings:ChannelProfile"] ?? throw new Exception("Invalid temp path.");
                    string baseDirectory = _configuration["appSettings:UploadPath"] ?? throw new Exception("Invalid UploadPath");
                   
                    folderPath = baseDirectory + folderPath;//flodrer import
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    string filename = Guid.NewGuid().ToString() + "." + ".png";
                    string base64Str = updoadReqeust.base64data!;
                    byte[] bytes = Convert.FromBase64String(base64Str!);

                    string filePath = Path.Combine(folderPath, filename);
                    if (filePath.Contains(".."))
                    { //if found .. in the file name or path
                        Log.Error("Invalid path " + filePath);
                        throw new Exception("Invalid path");
                    }
                    await System.IO.File.WriteAllBytesAsync(filePath, bytes);

                Result<string> resDa = await _blChannel.UploadProfile(LoginEmpID, ChannelId, filename, updoadReqeust.description);
                    if (resDa.IsError) return resDa;

                    if (!System.IO.File.Exists(filePath))
                    {
                        throw new Exception("Empty File.");
                    }
                    else
                    {
                        byte[] imageBytes = System.IO.File.ReadAllBytes(filePath);
                        string base64String = Convert.ToBase64String(imageBytes);
                        resDa.Data = base64String;
                        return resDa;
                    }
            }
            else
                throw new Exception("No File.");

        }
        catch (Exception ex)
        {
            return Result<string>.Error(ex);
        }
    }

    [HttpPost("GenerateChannelUrl",Name = "GenerateChannelUrl")]
    public async Task<Result<string>> GenerateChannelUrl(GetChannelData getChannelData)
    {
        try
        {
            string? channelIdval = getChannelData.channelIdval;
            if (string.IsNullOrEmpty(channelIdval)) throw new Exception("Channel Id can't null or empty");
            int LoginEmpID = Convert.ToInt32(_tokenData.LoginEmpID);
            int ChannelId = Convert.ToInt32(Encryption.DecryptID(channelIdval, LoginEmpID.ToString()));
            return await _blChannel.GenerateChannelUrl(ChannelId, LoginEmpID);
        }
        catch (Exception ex)
        {
            return Result<string>.Error(ex);
        }

    }

    [HttpPost("GenerateChannelQrCode",Name = "GenerateChannelQrCode")]
    public async Task<Result<string>> GenerateChannelQrCode(GetChannelData getChannelData)
    {
        try
        {
            string? channelIdval = getChannelData.channelIdval;
            if (string.IsNullOrEmpty(channelIdval)) throw new Exception("Channel Id can't null or empty");
            int LoginEmpID = Convert.ToInt32(_tokenData.LoginEmpID);
            int ChannelId = Convert.ToInt32(Encryption.DecryptID(channelIdval, LoginEmpID.ToString()));
            Result<string> resData = await _blChannel.GenerateChannelUrl(ChannelId, LoginEmpID);
            if (resData.IsError) return resData;

            // Create a new instance of the QR Code generator
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(resData.Data, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(20);

            // Convert the QR code image to a Base64 string
            string base64String;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                qrCodeImage.Save(memoryStream, ImageFormat.Png);
                byte[] imageBytes = memoryStream.ToArray();
                base64String = Convert.ToBase64String(imageBytes);
            }
            resData.Data = base64String;
            return resData;
        }
        catch (Exception ex)
        {
            return Result<string>.Error(ex);
        }
    }


    [HttpPost("VisitChannelByInviteLink",Name = "VisitChannelByInviteLink")]
    public async Task<Result<VisitChannelResponse>> VisitChannelByInviteLink(ChannelInviteLinkPayload payload)
    {
        if (string.IsNullOrEmpty(payload.InviteLink)) return Result<VisitChannelResponse>.Error("Invite  Link Can't Null Or Empty");
        int LoginEmpID = Convert.ToInt32(_tokenData.LoginEmpID);
        return await _blChannel.VisitChannelByInviteLink(payload.InviteLink, LoginEmpID);
    }


    [HttpPost("JoinChannelByInviteLink",Name = "JoinChannelByInviteLink")]
    public async Task<Result<string>> JoinChannelByInviteLink(JoinChannelInviteLinkPayload payload)
    {
        if (string.IsNullOrEmpty(payload.InviteLink)) return Result<string>.Error("Invite  Link Can't Null Or Empty");
        int LoginEmpID = Convert.ToInt32(_tokenData.LoginEmpID);
        return await _blChannel.JoinChannelByInviteLink(payload, LoginEmpID);
    }

    [HttpPost("GetChannelMember", Name = "GetChannelMember")]
    public async Task<Result<List<ChannelMemberResponse>>> GetChannelMember(GetMembershipPayload payload)
    {
        if (string.IsNullOrEmpty(payload.ChannelIdval) || string.IsNullOrEmpty(payload.MemberState)) return Result< List < ChannelMemberResponse >>.Error("Channel Id can't be empty or null");
        int LoginEmpID = Convert.ToInt32(_tokenData.LoginEmpID);
        return await _blChannel.GetChannelMember(payload.ChannelIdval,payload.MemberState.ToLower(), LoginEmpID);
    }

    [HttpPost("ApproveRejectChannelMember",Name = "ApproveRejectChannelMember")]
    public async Task<Result<string>> ApproveRejectChannelMember(List<AppRejChannelMemberPayload> payload)
    {
        int LoginEmpID = Convert.ToInt32(_tokenData.LoginEmpID);
        return await _blChannel.ApproveRejectChannelMember(payload, LoginEmpID);
    }
    [HttpPost("GetVisitUsersRecords",Name = "VisitUsersRecords")]
    public async Task<Result<List<VisitUserResponse>>> GetVisitUsersRecords(GetVisitUsersPayload payload)
    {
        int LoginEmpID = Convert.ToInt32(_tokenData.LoginEmpID);
        if (string.IsNullOrEmpty(payload.ChannelIdval) ||
            string.IsNullOrEmpty(payload.Date)) return Result<List<VisitUserResponse>>.Error("ChannelId or Viewed Date can't be null or empty");
        return await _blChannel.GetVisitUsersRecords(payload,LoginEmpID);
    }
    ///LeaveChannel
    ///Visit Users in a month
    ///New Member in a month
}
