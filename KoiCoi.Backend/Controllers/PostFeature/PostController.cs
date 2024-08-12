
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

    [HttpPost("ReviewPostsList",Name ="ReviewPostsList")]
    public async Task<Result<List<ReviewPostResponse>>> ReviewPostsList(ReviewPostPayload payload)
    {
        if (string.IsNullOrEmpty(payload.EventIdval) || string.IsNullOrEmpty(payload.Status)) return Result<List<ReviewPostResponse>>.Error("payload Can't Empty or null");
        int LoginUserID = Convert.ToInt32(_tokenData.LoginUserId);
        return await _blPost.ReviewPostsList(payload.EventIdval,payload.Status, LoginUserID);
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
     

    ///CreatePost
    ///ReviewPost
    ///ApproveOrRejectPost
    ///
}
