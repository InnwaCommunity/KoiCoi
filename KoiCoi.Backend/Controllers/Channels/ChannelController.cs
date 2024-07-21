using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QRCoder;
using Serilog;
using System.Configuration;
using System.Drawing.Imaging;
using System.Drawing;

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
    public async Task<ResponseData> GetCurrencyList()
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
    public async Task<ResponseData> CreateChannel(CreateChannelReqeust channelReqeust)
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
            ResponseData responseData = new ResponseData();
            responseData.StatusCode = 0;
            responseData.Message= ex.Message;
            return responseData;
        }
    }

    [HttpGet("GetChannels",Name = "GetChannels")]
    public async Task<ResponseData> GetChannels()
    {

        int LoginEmpID = Convert.ToInt32(_tokenData.LoginEmpID);
        return await _blChannel.GetChannels(LoginEmpID);

    }

    [HttpPost("GetChannelProfile",Name = "GetChannelProfile")]
    public async Task<ResponseData> GetChannelProfile(GetChannelData getChannelData)
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
            ResponseData responseData = new ResponseData();
            responseData.StatusCode = 0;
            responseData.Message = ex.Message;
            return responseData;
        }

    }
    [HttpPost("DirectUploadChannelProfile",Name = "DirectUploadChannelProfile")]

    public async Task<ResponseData> DirectUploadChannelProfile(UploadChannelProfileRequest updoadReqeust)
    {
        Response.ContentType = "application/json";
        ResponseData responseData = new ResponseData();
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

                    ResponseData resDa= await _blChannel.UploadProfile(LoginEmpID, ChannelId, filename, updoadReqeust.description);
                    if (resDa.StatusCode == 0) return resDa;

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
            responseData.StatusCode = 0;
            responseData.Message = ex.Message;
            return responseData;
        }
    }

    [HttpPost("GenerateChannelUrl",Name = "GenerateChannelUrl")]
    public async Task<ResponseData> GenerateChannelUrl(GetChannelData getChannelData)
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
            ResponseData res= new ResponseData();
            res.StatusCode = 0;
            res.Message = ex.Message;
            return res;
        }

    }

    [HttpPost("GenerateChannelQrCode",Name = "GenerateChannelQrCode")]
    public async Task<ResponseData> GenerateChannelQrCode(GetChannelData getChannelData)
    {
        try
        {
            string? channelIdval = getChannelData.channelIdval;
            if (string.IsNullOrEmpty(channelIdval)) throw new Exception("Channel Id can't null or empty");
            int LoginEmpID = Convert.ToInt32(_tokenData.LoginEmpID);
            int ChannelId = Convert.ToInt32(Encryption.DecryptID(channelIdval, LoginEmpID.ToString()));
            ResponseData resData= await _blChannel.GenerateChannelUrl(ChannelId, LoginEmpID);
            if (resData.StatusCode == 0) return resData;

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
            ResponseData res = new ResponseData();
            res.StatusCode = 0;
            res.Message = ex.Message;
            return res;
        }
    }


    [HttpPost("VisitChannelByInviteLink",Name = "VisitChannelByInviteLink")]
    public async Task<ResponseData> VisitChannelByInviteLink(ChannelInviteLinkPayload payload)
    {
        int LoginEmpID = Convert.ToInt32(_tokenData.LoginEmpID);
        return await _blChannel.VisitChannelByInviteLink(payload.InviteLink, LoginEmpID);
    }

}
