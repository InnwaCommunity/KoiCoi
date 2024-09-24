
using KoiCoi.Models.EventDto.Payload;
using KoiCoi.Models.Login_Models;
using System.Drawing.Printing;

namespace KoiCoi.Modules.Repository.PostFeature;

public  class BL_Post
{
    private readonly DA_Post _daPost;

    public BL_Post(DA_Post daPost)
    {
        _daPost = daPost;
    }

    public async Task<Result<string>> CreatePostFeature(CreatePostPayload payload,int LoginUserId)
    {
        return await _daPost.CreatePostFeature(payload, LoginUserId);
    }
    public async Task<Result<string>> UploadCollectAttachFile(PostImagePayload payload,int LoginUserID)
    {
        return await _daPost.UploadCollectAttachFile(payload, LoginUserID);
    }
    public async Task<Result<Pagination>> ReviewPostsList(ReviewPostPayload payload, int LoginUserId)
    {
        return await _daPost.ReviewPostsList(payload, LoginUserId);
    }

    public async Task<Result<string>> ApproveOrRejectPost(List<ApproveRejectPostPayload> payload,int LoginUserId)
    {
        return await _daPost.ApproveOrRejectPost(payload,LoginUserId);
    }

    /// <summary>
    /// Will Response Posts for Dashboard
    /// </summary>
    /// <param name="LoginUserId"></param>
    /// <returns></returns>
    
     public async Task<Result<List<DashboardPostsResponse>>> GetDashboardPosts(int LoginUserId,int pageNumber,int pageSize)
    {
        return await _daPost.GetDashboardPosts(LoginUserId, pageNumber, pageSize);
    }

    public async Task<Result<Pagination>> GetPostsOrderByEvent(GetEventData payload,int LoginUserID)
    {
        return await _daPost.GetPostsOrderByEvent(payload, LoginUserID);
    }
    public async Task<Result<string>> CreateEventTags(CreateEventTagListPayload payload,int LoginUserId)
    {
        return await _daPost.CreateEventTags(payload, LoginUserId);
    }

    public async Task<Result<List<PostTagDataResponse>>> GetEventTags(string EventPostIdval,int LoginUserId)
    {
        return await _daPost.GetEventTags(EventPostIdval, LoginUserId);
    }

}
