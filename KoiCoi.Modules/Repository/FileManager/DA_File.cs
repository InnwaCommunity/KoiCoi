using KoiCoi.Modules.Repository.UserFeature;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.Extensions.Configuration;
using System.Linq.Expressions;

namespace KoiCoi.Modules.Repository.FileManager;

public class DA_File
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _configuration;
    private readonly KcAwsS3Service _awsS3Service;

    public DA_File(AppDbContext db, IConfiguration configuration, KcAwsS3Service awsS3Service)
    {
        _db = db;
        _configuration = configuration;
        _awsS3Service = awsS3Service;
    }

    public async Task<Result<string>> GetFiles(GetFilePayload paylod, int LoginUserId)
    {
        Result<string> result = null;
        try
        {
            ///roles
            ///User Profiles(user)
            ///Channel Profile(channel)
            ///EventAttachFile(epost)
            ///Colllect Post Attach File(cpost)
            ///Usage Post Attach File(upost)
            if (!string.IsNullOrEmpty(paylod.Role) && !string.IsNullOrEmpty(paylod.Url))
            {

                switch (paylod.Role.ToLower())
                {
                    case "user":
                        result = await GetUserProfile(paylod.Url);
                        break;
                    case "channel":
                        result = await GetChannelProfile(paylod.Url);
                        break;
                    case "epost":
                        result = await GetEventAttachFile(paylod.Url);
                        break;
                    case "cpost":
                        result = await GetPostAttachFile(paylod.Url);
                        break;
                    default:
                        result = Result<string>.Error("This Role Not Found");
                        break;
                }
            }
            else
            {
                result = Result<string>.Error("Role Name or Url Is Null Or Empty");
            }
        }
        catch (Exception ex) {
            result = Result<String>.Error(ex);
        }
        return result;
    }

    private async Task<Result<string>> GetUserProfile(string url)
    {
        string bucketname = _configuration.GetSection("Buckets:UserProfile").Get<string>()!;
        return await _awsS3Service.GetFile(bucketname, url);
    }
    private async Task<Result<string>> GetChannelProfile(string url)
    {
        string bucketname = _configuration.GetSection("Buckets:ChannelProfile").Get<string>()!;
        return await _awsS3Service.GetFile(bucketname, url);
    }
    private async Task<Result<string>> GetEventAttachFile(string url)
    {
        string bucketname = _configuration.GetSection("Buckets:EventImages").Get<string>()!;
        return await _awsS3Service.GetFile(bucketname, url);
    }
    private async Task<Result<string>> GetPostAttachFile(string url)
    {
        string bucketname = _configuration.GetSection("Buckets:PostImages").Get<string>()!;
        return await _awsS3Service.GetFile(bucketname, url);
    }
}
