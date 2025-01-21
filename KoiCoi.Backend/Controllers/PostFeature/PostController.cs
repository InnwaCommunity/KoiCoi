
using KoiCoi.Models;
using Serilog;
using System.Collections.Generic;

namespace KoiCoi.Backend.Controllers.PostFeature;

[Route("api/[controller]")]
[ApiController]
public class PostController : BaseController
{
    private readonly IConfiguration _configuration;
    private readonly BL_Post _blPost;

    public PostController(BL_Post blPost, IConfiguration configuration)
    {
        _blPost = blPost;
        _configuration = configuration;
    }

    [HttpPost("CreatePostFeature",Name = "CreatePostFeature")]
    public async Task<Result<string>> CreatePostFeature(CreatePostPayload payload)
    {
        int LoginUserID = Convert.ToInt32(_tokenData.LoginUserId);
        return await _blPost.CreatePostFeature(payload, LoginUserID);
    }

    [RequestSizeLimit(100 * 1024 * 1024)]
    [HttpPost("UploadCollectAttachFile", Name = "UploadCollectAttachFile")]
    public async Task<Result<string>> UploadCollectAttachFile([FromForm] string PostIdval, [FromForm] IFormFile files)
    {
        try
        {
            if (files != null && files.Length > 0)
            {
                int LoginUserID = Convert.ToInt32(_tokenData.LoginUserId);
                return await _blPost.UploadCollectAttachFile(files, PostIdval, LoginUserID);
            }
            else
            {
               return Result<string>.Error("No file uploaded.");
            }
        }
        catch (Exception ex)
        {
            return Result<string>.Error(ex.Message);
        }
    }


    [HttpPost("ReviewPostsList",Name ="ReviewPostsList")]
    public async Task<Result<Pagination>> ReviewPostsList(ReviewPostPayload payload)
    {
        if (string.IsNullOrEmpty(payload.EventPostIdval) || string.IsNullOrEmpty(payload.Status)) return Result<Pagination>.Error("payload Can't Empty or null");
        int LoginUserID = Convert.ToInt32(_tokenData.LoginUserId);
        return await _blPost.ReviewPostsList(payload, LoginUserID);
    }
    [HttpPost("ApproveOrRejectPost",Name = "ApproveOrRejectPost")]
    public async Task<Result<string>> ApproveOrRejectPost(List<ApproveRejectPostPayload> payload)
    {
        int LoginUserID = Convert.ToInt32(_tokenData.LoginUserId);
        return await _blPost.ApproveOrRejectPost(payload,LoginUserID);
    }

    ///[Spiecal Get For Dashboard]///
    [HttpGet("{pageNumber}/{pageSize}")]
    public async Task<Result<Pagination>> GetDashboardPosts(int pageNumber, int pageSize)
    {
        int LoginUserID = Convert.ToInt32(_tokenData.LoginUserId);
        return await _blPost.GetDashboardPosts(LoginUserID, pageNumber, pageSize);
    }
    [HttpPost("GetPostsOrderByEvent", Name = "GetPostsOrderByEvent")]
    public async Task<Result<Pagination>> GetPostsOrderByEvent(GetEventData payload)
    {
        int LoginUserID = Convert.ToInt32(_tokenData.LoginUserId);
        return await _blPost.GetPostsOrderByEvent(payload, LoginUserID);
    }

    [HttpPost("GetEachUserPosts", Name = "GetEachUserPosts")]
    public async Task<Result<Pagination>> GetEachUserPosts(GetEachUserPostsPayload payload)
    {
        int LoginUserID = Convert.ToInt32(_tokenData.LoginUserId);
        return await _blPost.GetEachUserPosts(LoginUserID, payload);
    }
    [HttpDelete("DeletePost",Name = "DeletePost")]
    public async Task<Result<string>> DeletePost(DeletePayload payload)
    {
        int LoginUserID = Convert.ToInt32(_tokenData.LoginUserId);
        return await _blPost.DeletePost(LoginUserID, payload.ItemIdToDelete);
    }
    ///CreatePost
    ///ReviewPost
    ///ApproveOrRejectPost
    ///
}
