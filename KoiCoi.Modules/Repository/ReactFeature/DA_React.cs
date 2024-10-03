

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

    public async Task<Result<string>> ReactPost(ReactPostPayload payload, int LoginUserID)
    {
        Result<string> result = null;
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
                    result = Result<string>.Success("React Success");
}
                else
                {
                    ///update React
                    react.ReactTypeId = reacttypeid;
                    await _db.SaveChangesAsync();
                    result = Result<string>.Success("Update React Success");
                }
            }
            else
            {
                ///Delete React
                 if(react is not null)
                {
                    _db.Reacts.Remove(react);
                    await _db.SaveChangesAsync();
                }
                result = Result<string>.Success("Delete React Success");

            }
        }
        catch (Exception ex)
        {
            result = Result<string>.Error(ex);
        }
        return result;
    }

    public async Task<Result<string>> CommentPost(CommentPostPayload payload, int LoginUserID)
    {
        Result<string> result = null;
        try
        {
            if (payload.PostIdval is null && payload.ParentIdval is null)
                return Result<string>.Error("Post can be null");

            if(payload.PostIdval is not null)
            {
                int PostId = Convert.ToInt32(Encryption.DecryptID(payload.PostIdval, LoginUserID.ToString()));
                var post = await _db.Posts.Where(x=> x.PostId== PostId).FirstOrDefaultAsync();
                if(post is null)
                    return Result<string>.Error("Post can't found");
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
                result = Result<string>.Success("Success");
            }
            else if(payload.ParentIdval is not null)
            {
                int ParentCommandId = Convert.ToInt32(Encryption.DecryptID(payload.ParentIdval, LoginUserID.ToString()));
                var post = await (from _pt in _db.Posts
                                  join _cop in _db.PostCommands on _pt.PostId equals _cop.PostId
                                  where _cop.CommandId == ParentCommandId
                                  select _pt).FirstOrDefaultAsync();
                if (post is null)
                    return Result<string>.Error("Post can't found");
                PostCommand newCommand = new PostCommand
                {
                    Content = payload.Content,
                    PostId = post.PostId,
                    UserId = LoginUserID,
                    ModifiedDate = DateTime.UtcNow,
                    CreatedDate = DateTime.UtcNow,
                };
                await _db.PostCommands.AddAsync(newCommand);
                await _db.SaveChangesAsync();
                result = Result<string>.Success("Success");
            }
            result = Result<string>.Error("Fail");
        }
        catch (Exception ex)
        {
            result = Result<string>.Error(ex);
        }
        return result;
    }
    public async Task<Result<Pagination>> GetComments(GetCommentPayload payload, int LoginUserId)
    {
        Result<Pagination> result = null;
        try
        {
            if (string.IsNullOrEmpty(payload.PostIdval))
                return Result<Pagination>.Error("Post Not Found");
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
                               where _post.PostId == PostId && ParentCommendId != null ? ParentCommendId == _com.ParentCommandId : true
                               select new
                               {
                                   CommandIdval = Encryption.EncryptID(_com.CommandId.ToString(), LoginUserId.ToString()),
                                   Content = _com.Content,
                                   CreatorIdval = Encryption.EncryptID(_creator.UserId.ToString(), LoginUserId.ToString()),
                                   CreatorName = _creator.Name,
                                   CreatorEmail = _creator.Email,
                                   CanEdit = _creator.UserId == LoginUserId,
                                   CreatorImage = profiles.OrderByDescending(p => p.CreatedDate).Select(x=> x.Url).FirstOrDefault(),
                                   HaveChildCommand = _db.PostCommands.Where(x=> x.ParentCommandId == _com.CommandId).FirstOrDefault() != null
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
}
