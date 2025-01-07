using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using KoiCoi.Database.AppDbContextModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using NuGet.ProjectModel;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

namespace KoiCoi.Operational.Services;

public class KcAwsS3Service
{
    private readonly IConfiguration _configuration;
    private readonly AmazonS3Client _s3Client;

    public KcAwsS3Service(IConfiguration configuration)
    {
        _configuration = configuration;
        string accessKey = _configuration.GetSection("AWSS3:Accesskey").Get<string>()!;
        string serectKey = _configuration.GetSection("AWSS3:SecretAccessKey").Get<string>()!;
        var credentials = new BasicAWSCredentials(accessKey, serectKey);
        _s3Client = new AmazonS3Client(credentials, Amazon.RegionEndpoint.APSoutheast1);
    }
    public async Task<Result<string>> CreateFileAsync(IFormFile file,string bucketname, string key,string ext)
    {
        try
        {
            // Check if the bucket exists
            var bucketExists = await Amazon.S3.Util.AmazonS3Util.DoesS3BucketExistV2Async(_s3Client, bucketname);
            if (!bucketExists)
            {
                //create bucket
                await _s3Client.PutBucketAsync(bucketname);
            }

            // Validate the uploaded file
            if (file == null || file.Length == 0)
            {
                return Result<string>.Error("error");
            }

            // Determine the content type based on the file extension
            string? contentType = GetContentType(ext);

            if (contentType == null)
            {
                return Result<string>.Error("error");
            }

            // Upload the file to S3
            using (var stream = file.OpenReadStream())
            {
                var putRequest = new PutObjectRequest
                {
                    BucketName = bucketname,
                    Key = key,
                    InputStream = stream,
                    ContentType = contentType
                };

                var response = await _s3Client.PutObjectAsync(putRequest);

                return response.HttpStatusCode == System.Net.HttpStatusCode.OK ? Result<string>.Success("success") : Result<string>.Error("error");
            }
        }
        catch (AmazonS3Exception ex)
        {
            Console.WriteLine($"Amazon S3 Error: {ex.Message}");
            Console.WriteLine($"Error Code: {ex.ErrorCode}");
            Console.WriteLine($"Request ID: {ex.RequestId}");
            Console.WriteLine($"Status Code: {ex.StatusCode}");
            return Result<string>.Error(ex);
        }
        catch (Exception ex)
        {
            return Result<string>.Error(ex);
        }
    }

    // Helper method to determine the content type
    private string? GetContentType(string extension)
    {
        return extension.ToLower() switch
        {
            // Image formats
            ".pdf" => "application/pdf",
            ".jpg" => "image/jpeg",
            ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            ".svg" => "image/svg+xml",
            ".tiff" => "image/tiff",
            ".tif" => "image/tiff",
            ".ico" => "image/x-icon",
            ".webp" => "image/webp",
            ".heic" => "image/heic",

            // Audio formats
            ".mp3" => "audio/mpeg",
            ".wav" => "audio/wav",
            ".ogg" => "audio/ogg",
            ".aac" => "audio/aac",
            ".flac" => "audio/flac",
            ".m4a" => "audio/x-m4a",
            ".wma" => "audio/x-ms-wma",

            // Video formats
            ".mp4" => "video/mp4",
            ".mov" => "video/quicktime",
            ".avi" => "video/x-msvideo",
            ".wmv" => "video/x-ms-wmv",
            ".mkv" => "video/x-matroska",
            ".webm" => "video/webm",
            ".flv" => "video/x-flv",
            ".3gp" => "video/3gpp",
            ".m4v" => "video/x-m4v",

            // Spreadsheet formats
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".xls" => "application/vnd.ms-excel",
            ".csv" => "text/csv",

            _ => null,
        };
    }


    // Helper method to validate a base64 string
    private bool IsValidBase64String(string base64)
    {
        base64 = base64.Trim();
        return (base64.Length % 4 == 0) && Regex.IsMatch(base64, @"^[a-zA-Z0-9\+/]*={0,3}$", RegexOptions.None);
    }

    public async Task<Result<string>> GetFile(string bucketName, string key)
    {
        try
        {
            var bucketExists = await Amazon.S3.Util.AmazonS3Util.DoesS3BucketExistV2Async(_s3Client, bucketName);
            if (!bucketExists) return Result<string>.Error($"Bucket {bucketName} does not exist.");
            var request = new GetPreSignedUrlRequest
            {
                BucketName = bucketName,
                Key = key,
                Expires = DateTime.UtcNow.AddMinutes(3)
            };

            var url = _s3Client.GetPreSignedURL(request);
            return Result<string>.Success(url);

            /*var s3Object = await _s3Client.GetObjectAsync(bucketName, key);
            var file= File(s3Object.ResponseStream, s3Object.Headers.ContentType);
            return Resu
              using (var memoryStream = new MemoryStream())
            {
                await s3Object.ResponseStream.CopyToAsync(memoryStream);
                var fileBytes = memoryStream.ToArray();
                var base64String = Convert.ToBase64String(fileBytes);
                return Result<string>.Success(base64String);
            }
             */
        }
        catch (Exception ex)
        {
            return Result<string>.Error($"Error occurred while retrieving file: {ex.Message}");
        }
    }

}
