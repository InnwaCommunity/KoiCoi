using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using KoiCoi.Database.AppDbContextModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
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
    public async Task<string> CreateFileAsync(string base64,string bucketname, string key,string ext)
    {
        try
        {
            var bucketExists = await Amazon.S3.Util.AmazonS3Util.DoesS3BucketExistV2Async(_s3Client, bucketname);
            if (!bucketExists)
            {
                await _s3Client.PutBucketAsync(bucketname);
            }

            // Decode the base64 string
            byte[] fileBytes = Convert.FromBase64String(base64);

            // Validate the base64 string
            if (string.IsNullOrEmpty(base64) || !IsValidBase64String(base64))
            {
                return "error";
            }
            // Determine the content type based on the file extension
            string? contentType = GetContentType(ext);

            if (contentType == null)
            {
                return "error";
            }

            // Generate a unique file name
            //string fileName = $"{DateTime.Now:yyyy\\/MM\\/dd\\/}{Globalfunction.NewUniqueFileName}{ext}";

            // Create a MemoryStream from the byte array
            using (var stream = new MemoryStream(fileBytes))
            {
                var putRequest = new PutObjectRequest
                {
                    BucketName = bucketname,
                    Key = key,
                    InputStream = stream,
                    ContentType = contentType
                };

                var response = await _s3Client.PutObjectAsync(putRequest);

                return "success";
            }
        }
        catch (AmazonS3Exception ex)
        {
            return "error";
        }
        catch (Exception ex)
        {
            return "error";
        }
    }

    // Helper method to determine the content type
    private string? GetContentType(string extension)
    {
        return extension.ToLower() switch
        {
            ".pdf" => "application/pdf",
            ".jpg" => "image/jpeg",
            ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
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
}
