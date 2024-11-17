

using Amazon;
using KoiCoi.Database.AppDbContextModels;
using KoiCoi.Models;
using Microsoft.Extensions.Configuration;

namespace KoiCoi.Modules.Repository.ReactFeature;

public class DA_React
{
    private readonly AppDbContext _db;
    private readonly NotificationManager.NotificationManager _saveNotifications;
    private readonly IConfiguration _configuration;

    public DA_React(AppDbContext db, IConfiguration configuration, NotificationManager.NotificationManager saveNotifications)
    {
        _db = db;
        _configuration = configuration;
        _saveNotifications = saveNotifications;
    }


    public async Task<Result<List<ReactTypeResponse>>> GetAllReactType(int LoginUserId)
    {
        Result<List<ReactTypeResponse>> result = null;
        try
        {
            List<ReactTypeResponse> querylist = await _db.ReactTypes.Select(
                x => new ReactTypeResponse
                {
                    TypeIdval = Encryption.EncryptID(x.TypeId.ToString(), LoginUserId.ToString()),
                    Emoji = x.Emoji,
                    Description = x.Description
                }).ToListAsync();
            result = Result<List<ReactTypeResponse>>.Success(querylist);
        }
        catch (Exception ex)
        {
            result = Result<List<ReactTypeResponse>>.Error(ex);
        }
        return result;
    }

    public async Task<Result<int>> ReactPost(ReactPostPayload payload, int LoginUserID)
    {
        Result<int> result = null;
        try
        {
            int postId = Convert.ToInt32(Encryption.DecryptID(payload.postIdval!, LoginUserID.ToString()));
            var react = await _db.Reacts.Where(
                x => x.PostId == postId && x.UserId == LoginUserID)
                .FirstOrDefaultAsync();
            if (!string.IsNullOrEmpty(payload.reacttypeIdval))
            {
                int reacttypeid = Convert.ToInt32(Encryption.DecryptID(payload.reacttypeIdval, LoginUserID.ToString()));
                if(react is null)
                {
                    ///create React
                    React newreact = new React
                    {
                        PostId = postId,
                        UserId = LoginUserID,
                        ReactTypeId = reacttypeid,
                        CreatedDate = DateTime.UtcNow
                    };

                    await _db.Reacts.AddAsync(newreact);
                    await _db.SaveChangesAsync();
}
                else
                {
                    ///update React
                    react.ReactTypeId = reacttypeid;
                    await _db.SaveChangesAsync();
                }
            }
            else
            {
                ///Delete React
                 if(react is not null)
                {
                    _db.Reacts.Remove(react);
                    _db.Entry(react).State = EntityState.Deleted;
                    await _db.SaveChangesAsync();
                }

            }
            int reactCount = _db.Reacts.Where(x=> x.PostId == postId).Count();
            result = Result<int>.Success(reactCount);

        }
        catch (Exception ex)
        {
            result = Result<int>.Error(ex);
        }
        return result;
    }

    public async Task<Result<GetCommentResponse>> CommentPost(CommentPostPayload payload, int LoginUserID)
    {
        Result<GetCommentResponse> result = null;
        try
        {
            if (payload.PostIdval is null && payload.ParentIdval is null)
                return Result<GetCommentResponse>.Error("Post can be null");

            if(payload.PostIdval is not null)
            {
                int PostId = Convert.ToInt32(Encryption.DecryptID(payload.PostIdval, LoginUserID.ToString()));
                var post = await _db.Posts.Where(x=> x.PostId== PostId).FirstOrDefaultAsync();
                if(post is null)
                    return Result<GetCommentResponse>.Error("Post can't found");
                if (payload.ParentIdval is not null)
                {
                    int ParentCommandId = Convert.ToInt32(Encryption.DecryptID(payload.ParentIdval, LoginUserID.ToString()));
                    var parentCommand = await _db.PostCommands.Where(x=> x.CommandId == ParentCommandId).FirstOrDefaultAsync();
                        
                    if (parentCommand is null)
                        return Result<GetCommentResponse>.Error("Parent Command can't found");
                    PostCommand newCommand = new PostCommand
                    {
                        Content = payload.Content,
                        PostId = post.PostId,
                        UserId = LoginUserID,
                        ParentCommandId = ParentCommandId,
                        ModifiedDate = DateTime.UtcNow,
                        CreatedDate = DateTime.UtcNow,
                    };
                    await _db.PostCommands.AddAsync(newCommand);
                    await _db.SaveChangesAsync();
                    var query = await (from _com in _db.PostCommands
                                       join _creator in _db.Users on _com.UserId equals _creator.UserId
                                       join pro in _db.UserProfiles on _creator.UserId equals pro.UserId into profiles
                                       join _cmr in _db.CommandReacts on _com.CommandId equals _cmr.CommandId into cmreacts
                                       where _com.CommandId == newCommand.CommandId
                                       select new GetCommentResponse
                                       {
                                           CommandIdval = Encryption.EncryptID(_com.CommandId.ToString(), LoginUserID.ToString()),
                                           Content = _com.Content,
                                           CreatorIdval = Encryption.EncryptID(_creator.UserId.ToString(), LoginUserID.ToString()),
                                           CreatorName = _creator.Name,
                                           CreatorEmail = _creator.Email,
                                           CanEdit = _creator.UserId == LoginUserID,
                                           CreateData = _com.CreatedDate,
                                           ReactCount = cmreacts.Count(),
                                           Selected = _db.CommandReacts.Where(x => x.UserId == LoginUserID && x.CommandId == _com.CommandId).FirstOrDefault() != null,
                                           CreatorImage = profiles.OrderByDescending(p => p.CreatedDate).Select(x => x.Url).FirstOrDefault(),
                                           HaveChildCommand = _db.PostCommands.Any(x => x.ParentCommandId == _com.CommandId)
                                       }).FirstOrDefaultAsync();
                    if(query is null)
                        return Result<GetCommentResponse>.Error("Fail"); 
                    result = Result<GetCommentResponse>.Success(query);
                }
                else
                {
                    PostCommand newCommand = new PostCommand
                    {
                        Content = payload.Content,
                        PostId = PostId,
                        UserId = LoginUserID,
                        ModifiedDate = DateTime.UtcNow,
                        CreatedDate = DateTime.UtcNow,
                    };
                    await _db.PostCommands.AddAsync(newCommand);
                    await _db.SaveChangesAsync();
                    var query = await (from _com in _db.PostCommands
                                       join _creator in _db.Users on _com.UserId equals _creator.UserId
                                       join pro in _db.UserProfiles on _creator.UserId equals pro.UserId into profiles
                                       join _cmr in _db.CommandReacts on _com.CommandId equals _cmr.CommandId into cmreacts
                                       where _com.CommandId == newCommand.CommandId
                                       select new GetCommentResponse
                                       {
                                           CommandIdval = Encryption.EncryptID(_com.CommandId.ToString(), LoginUserID.ToString()),
                                           Content = _com.Content,
                                           CreatorIdval = Encryption.EncryptID(_creator.UserId.ToString(), LoginUserID.ToString()),
                                           CreatorName = _creator.Name,
                                           CreatorEmail = _creator.Email,
                                           CanEdit = _creator.UserId == LoginUserID,
                                           CreateData = _com.CreatedDate,
                                           ReactCount = cmreacts.Count(),
                                           Selected = _db.CommandReacts.Where(x => x.UserId == LoginUserID && x.CommandId == _com.CommandId).FirstOrDefault() != null,
                                           CreatorImage = profiles.OrderByDescending(p => p.CreatedDate).Select(x => x.Url).FirstOrDefault(),
                                           HaveChildCommand = _db.PostCommands.Any(x => x.ParentCommandId == _com.CommandId)
                                       }).FirstOrDefaultAsync();
                    if (query is null)
                        return Result<GetCommentResponse>.Error("Fail");
                    result = Result<GetCommentResponse>.Success(query);
                }
            }
            else
            {
                result = Result<GetCommentResponse>.Error("Fail");
            }
        }
        catch (Exception ex)
        {
            result = Result<GetCommentResponse>.Error(ex);
        }
        return result;
    }
    public async Task<Result<Pagination>> GetComments(GetCommentPayload payload, int LoginUserId)
    {
        Result<Pagination> result = null;
        try
        {
            if (string.IsNullOrEmpty(payload.PostIdval))
                return Result<Pagination>.Error("Invalide Post");
            int PostId = Convert.ToInt32(Encryption.DecryptID(payload.PostIdval, LoginUserId.ToString()));
            int? ParentCommendId = null;
            if (!string.IsNullOrEmpty(payload.ParentCommandIdval))
            {
                ParentCommendId = Convert.ToInt32(Encryption.DecryptID(payload.ParentCommandIdval, LoginUserId.ToString()));
            }

            var query = await (from _post in _db.Posts
                               join _com in _db.PostCommands on _post.PostId equals _com.PostId
                               join _creator in _db.Users on _com.UserId equals _creator.UserId
                               join pro in _db.UserProfiles on _creator.UserId equals pro.UserId into profiles
                               where _post.PostId == PostId && (ParentCommendId != null ? ParentCommendId == _com.ParentCommandId : _com.ParentCommandId==null)
                               select new GetCommentResponse
                               {
                                   CommandIdval = Encryption.EncryptID(_com.CommandId.ToString(), LoginUserId.ToString()),
                                   Content = _com.Content,
                                   CreatorIdval = Encryption.EncryptID(_creator.UserId.ToString(), LoginUserId.ToString()),
                                   CreatorName = _creator.Name,
                                   CreatorEmail = _creator.Email,
                                   CanEdit = _creator.UserId == LoginUserId,
                                   CreateData = _com.CreatedDate,
                                   ReactCount = _db.CommandReacts.Count(r => r.CommandId == _com.CommandId),
                                   Selected = _db.CommandReacts.Where(x => x.UserId == LoginUserId && x.CommandId == _com.CommandId).FirstOrDefault() != null,
                                   CreatorImage = profiles.OrderByDescending(p => p.CreatedDate).Select(x => x.Url).FirstOrDefault(),
                                   HaveChildCommand = _db.PostCommands.Any(x => x.ParentCommandId == _com.CommandId)
                               }).ToListAsync();
            Pagination pagination = RepoFunService.getWithPagination(payload.pageNumber, payload.pageSize, query);
            result = Result<Pagination>.Success(pagination);
        }
        catch (Exception ex)
        {
            result = Result<Pagination>.Error(ex);
        }
        return result;
    }

    public async Task<Result<string>> UpdateComment(UpdateCommentPayload payload, int LoginUserID)
    {
        Result<string> result = null;
        try
        {
            if (string.IsNullOrEmpty(payload.CommentIdval))
                return Result<string>.Error("Invalide Comment Id");
            int CommentId = Convert.ToInt32(Encryption.DecryptID(payload.CommentIdval, LoginUserID.ToString()));
            var comment = await _db.PostCommands.Where(x => x.CommandId == CommentId).FirstOrDefaultAsync();
            if (comment is null)
                return Result<string>.Error("Comment Not Found");

            comment.Content = payload.Content;
            comment.ModifiedDate = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            result= Result<string>.Success("Success");
        }
        catch (Exception ex)
        {
            result = Result<string>.Error(ex.Message);
        }
        return result;
    }

    public async Task<Result<string>> DeleteComment(DeleteCommentPayload payload, int LoginUserID)
    {
        Result<string> result = null;
        try
        {
            if (string.IsNullOrEmpty(payload.CommentIdval))
                return Result<string>.Error("Invalide Comment Id");
            int CommentId = Convert.ToInt32(Encryption.DecryptID(payload.CommentIdval, LoginUserID.ToString()));
            var comment = await _db.PostCommands.Where(x => x.CommandId == CommentId).FirstOrDefaultAsync();
            if (comment is null)
                return Result<string>.Error("Comment Not Found");

            _db.PostCommands.Remove(comment);
            _db.Entry(comment).State = EntityState.Deleted;
            await _db.SaveChangesAsync();
            result = Result<string>.Success("Update Success");
        }
        catch (Exception ex)
        {
            result = Result<string>.Error(ex.Message);
        }
        return result;
    }

    public async Task<Result<int>> ReactComment(ReactCommentPayload payload, int LoginUserID)
    {

        Result<int> result = null;
        try
        {
            int commentId = Convert.ToInt32(Encryption.DecryptID(payload.commentIdval!, LoginUserID.ToString()));
            var react = await _db.CommandReacts.Where(
                x => x.CommandId == commentId && x.UserId == LoginUserID)
                .FirstOrDefaultAsync();
            if (!string.IsNullOrEmpty(payload.reacttypeIdval))
            {
                int reacttypeid = Convert.ToInt32(Encryption.DecryptID(payload.reacttypeIdval, LoginUserID.ToString()));
                if (react is null)
                {
                    ///create React
                    CommandReact newreact = new CommandReact
                    {
                        CommandId = commentId,
                        UserId = LoginUserID,
                        ReactTypeId = reacttypeid,
                        CreatedDate = DateTime.UtcNow
                    };

                    await _db.CommandReacts.AddAsync(newreact);
                    await _db.SaveChangesAsync();
                }
                else
                {
                    ///update React
                    react.ReactTypeId = reacttypeid;
                    await _db.SaveChangesAsync();
                }
            }
            else
            {
                ///Delete React
                if (react is not null)
                {
                    _db.CommandReacts.Remove(react);
                    _db.Entry(react).State = EntityState.Deleted;
                    await _db.SaveChangesAsync();
                }

            }
            int reactCount = _db.CommandReacts.Where(x => x.CommandId == commentId).Count();
            result = Result<int>.Success(reactCount);

        }
        catch (Exception ex)
        {
            result = Result<int>.Error(ex);
        }
        return result;
    }
}
