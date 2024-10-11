
using KoiCoi.Models.Login_Models;
using KoiCoi.Modules.Repository.PostFeature;

namespace KoiCoi.Modules.Repository.ReactFeature;
public class BL_React
{
    private readonly DA_React _daReact;

    public BL_React(DA_React daReact)
    {
        _daReact = daReact;
    }

    public async Task<Result<List<ReactTypeResponse>>> GetAllReactType(int LoginUserId)
    {
        return await _daReact.GetAllReactType(LoginUserId);
    }

    public async Task<Result<int>> ReactPost(ReactPostPayload payload,int LoginUserID)
    {
        return await _daReact.ReactPost(payload, LoginUserID);
    }

    public async Task<Result<GetCommentResponse>> CommentPost(CommentPostPayload payload,int LoginUserID)
    {
        return await _daReact.CommentPost(payload, LoginUserID);
    }
    public async Task<Result<Pagination>> GetComments(GetCommentPayload payload,int LoginUserId)
    {
        return await _daReact.GetComments(payload, LoginUserId);
    }
    public async Task<Result<string>> UpdateComment(UpdateCommentPayload payload,int LoginUserID)
    {
        return await _daReact.UpdateComment(payload, LoginUserID);
    }
    public async Task<Result<string>> DeleteComment(DeleteCommentPayload payload, int LoginUserID)
    {
        return await _daReact.DeleteComment(payload, LoginUserID);
    }

    public async Task<Result<int>> ReactComment(ReactCommentPayload payload, int LoginUserID)
    {
        return await _daReact.ReactComment(payload, LoginUserID);
    }
}
