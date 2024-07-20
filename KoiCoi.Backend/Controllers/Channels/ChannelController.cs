using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Configuration;

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

    [HttpPost("UploadChannelProfileTemp", Name = "UploadChannelProfileTemp")]
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
    [HttpPost("CreateChannel",Name = "CreateChannel")]
    public async Task<ResponseData> CreateChannel(CreateChannelReqeust channelReqeust)
    {
        try
        {
            string baseDirectory = _configuration["appSettings:UploadPath"] ?? throw new Exception("Invalid UploadPath");
            string tempfolderPath = _configuration["appSettings:UploadChannelProfilePath"] ?? throw new Exception("Invalid temp path.");
            string uploadDirectory = _configuration["appSettings:ChannelProfile"] ?? throw new Exception("Invalid function upload path.");
            if(!string.IsNullOrEmpty(channelReqeust.ProfileImgName) )
            {
                string destinationDirectory = Path.Combine(baseDirectory, uploadDirectory);
                string sourceFilePath = Path.Combine(baseDirectory, tempfolderPath, channelReqeust.ProfileImgName);
                string destinationFilePath = Path.Combine(destinationDirectory, channelReqeust.ProfileImgName);
                if (!System.IO.File.Exists(sourceFilePath))
                {
                     throw new Exception("Source file not found.");
                }

                if (!Directory.Exists(destinationDirectory))
                {
                    Directory.CreateDirectory(destinationDirectory);
                }

                System.IO.File.Copy(sourceFilePath, destinationFilePath, true);
                System.IO.File.Delete(sourceFilePath);
            }

            int LoginEmpID = Convert.ToInt32(_tokenData.LoginEmpID);
            return await _blChannel.CreateChannel(channelReqeust, LoginEmpID);
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
    public async Task<ResponseData> GetChannelProfile(GetChannelProfile getChannelProfile)
    {
        try
        {
            string? channelIdval = getChannelProfile.channelProfileIdval;
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

    public async Task<ResponseData> DirectUploadChannelProfile([FromForm] string channelId,[FromForm] string? imgDescription)
    {
        Response.ContentType = "application/json";
        ResponseData responseData = new ResponseData();
        try
        {
            int LoginEmpID = Convert.ToInt32(_tokenData.LoginEmpID);
            int ChannelId = Convert.ToInt32(Encryption.DecryptID(channelId, LoginEmpID.ToString()));

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
                    string folderPath = _configuration["appSettings:ChannelProfile"] ?? throw new Exception("Invalid temp path.");
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
                    fullPath = folderPath + filename;

                    if (fullPath.Contains(".."))
                    { //if found .. in the file name or path
                        Log.Error("Invalid path " + fullPath);
                        throw new Exception("Invalid path");
                    }

                    using (var fileStream = new FileStream(fullPath, FileMode.OpenOrCreate))
                    {
                        await file.CopyToAsync(fileStream);
                    }

                    ResponseData resDa= await _blChannel.UploadProfile(LoginEmpID, ChannelId, filename, imgDescription);
                    if (resDa.StatusCode == 0) return resDa;

                    if (!System.IO.File.Exists(fullPath))
                    {
                        throw new Exception("Empty File.");
                    }
                    else
                    {
                        byte[] imageBytes = System.IO.File.ReadAllBytes(fullPath);
                        string base64String = Convert.ToBase64String(imageBytes);
                        resDa.Data = base64String;
                        return resDa;
                    }
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

}
