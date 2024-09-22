
using KoiCoi.Models;
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
    [HttpPost("UploadCollectAttachFile",Name = "UploadCollectAttachFile")]
    public async Task<Result<string>> UploadCollectAttachFile(PostImagePayload payload)
    {
        int LoginUserID = Convert.ToInt32(_tokenData.LoginUserId);
        return await _blPost.UploadCollectAttachFile(payload, LoginUserID);
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
    public async Task<Result<List<DashboardPostsResponse>>> GetDashboardPosts(int pageNumber, int pageSize)
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
     

    ///CreatePost
    ///ReviewPost
    ///ApproveOrRejectPost
    ///
}
