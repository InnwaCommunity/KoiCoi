using KoiCoi.Database.AppDbContextModels;
using KoiCoi.Models.EventDto;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace KoiCoi.Modules.Repository.EventFreture;

public class DA_Event
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _configuration;

    public DA_Event(AppDbContext db, IConfiguration configuration)
    {
        _db = db;
        _configuration = configuration;
    }

    public async Task<Result<string>> CreateEvent(CreateEventPayload paylod,int LoginUserId)
    {
        Result<string> result = null;
        try
        {
            int ChannelId = Convert.ToInt32(Encryption.DecryptID(paylod.ChannelIdval!, LoginUserId.ToString()));
            int status = 0;
            var ownerusertype = await (from _me in _db.ChannelMemberships
                                       join _uset in _db.UserTypes on _me.UserTypeId equals _uset.TypeId
                                       where _me.ChannelId == ChannelId && _me.UserId == LoginUserId &&
                                       _uset.Name.ToLower() == "owner" 
                                       select new
                                       {
                                           UserId = _me.UserId,
                                       })
                                       .FirstOrDefaultAsync();
            if(ownerusertype is not null)
            {
                status = await _db.StatusTypes.Where(x => x.StatusName.ToLower() == "approved")
                    .Select(x => x.StatusId)
                    .FirstOrDefaultAsync();
            }
            else
            {
                status = await _db.StatusTypes.Where(x => x.StatusName.ToLower() == "pending")
                    .Select(x => x.StatusId)
                    .FirstOrDefaultAsync();
            }
            if (status is 0) return Result<string>.Error("Pending Status Not Found");
            Event newEvent = new Event
            {
                EventName = paylod.EventName!,
                EventDescription = paylod.EventDescription,
                ChannelId = ChannelId,
                CreatorId = LoginUserId,
                ApproverId = ownerusertype is not null ? LoginUserId : null,
                StatusId = status,
                TotalBalance = "0",
                LastBalance = "0",
                StartDate = DateTime.Parse(paylod!.StartDate!),
                EndDate = DateTime.Parse(paylod!.EndDate!),
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow,
                Inactive = false
            };
            var res = await _db.Events.AddAsync(newEvent);
            await _db.SaveChangesAsync();
            result = Result<string>.Success("Requested Event Success");

            if(paylod.EventPhotos.Any())
            {
                string baseDirectory = _configuration["appSettings:UploadPath"] ?? throw new Exception("Invalid UploadPath");
                string uploadDirectory = _configuration["appSettings:EventImages"] ?? throw new Exception("Invalid function upload path.");
                string destDirectory = Path.Combine(baseDirectory, uploadDirectory);
                if (!Directory.Exists(destDirectory))
                {
                    Directory.CreateDirectory(destDirectory);
                }
                foreach (var item in paylod.EventPhotos)
                {
                    string filename = Guid.NewGuid().ToString() + "." + ".png";
                    string base64Str = item.base64image!;
                    byte[] bytes = Convert.FromBase64String(base64Str!);

                    string filePath = Path.Combine(destDirectory, filename);
                    if (filePath.Contains(".."))
                    { //if found .. in the file name or path
                        Log.Error("Invalid path " + filePath);
                    }
                    await System.IO.File.WriteAllBytesAsync(filePath, bytes);

                    var newImage = new EventImage
                    {
                        Url = filename,
                        UrlDescription = item.Description,
                        EventId= newEvent.Eventid,
                        CreatedDate = DateTime.UtcNow,
                        ModifiedDate = DateTime.UtcNow,
                    }; 
                    await _db.EventImages.AddAsync(newImage);
                    await _db.SaveChangesAsync();
                }
            }
            if (ownerusertype is not null)
            {
                ///Created the Event by owner
                List<int> channelMember = await _db.ChannelMemberships.Where(x=> x.ChannelId == ChannelId)
                    .Select(x=> x.UserId).ToListAsync();
                if (channelMember.Contains(LoginUserId))
                {
                    channelMember.Remove(LoginUserId);
                }
                SaveNotification(channelMember,
                    LoginUserId,
                    $"Upcoming the New Event {newEvent.EventName}",
                    newEvent.EventDescription,
                    $"UpcomingNewEvent/{newEvent.Eventid}");
            }
            else
            {
                ///Pending the Event 
                List<int> admins = await (from _meme in _db.ChannelMemberships
                                          join _usertype in _db.UserTypes on _meme.UserId equals _usertype.TypeId
                                          where _meme.ChannelId == ChannelId &&
                                          (_usertype.Name.ToLower() == "owner" || _usertype.Name.ToLower() == "admin")
                                          select _meme.UserId).ToListAsync();
                if (admins.Contains(LoginUserId))
                {
                    admins.Remove(LoginUserId);
                }
                string? LoginUserName= await _db.Users.Where(x=> x.UserId == LoginUserId)
                    .Select(x=> x.Name).FirstOrDefaultAsync();
                SaveNotification(admins,
                    LoginUserId,
                    $"Requested the New Event {newEvent.EventName} by Member {LoginUserName}",
                    newEvent.EventDescription,
                    $"RequestedNewEvent/{newEvent.Eventid}");
            }
        }
        catch (Exception ex)
        {
            result = Result<string>.Error(ex);
        }

        return result;
    }

    private async void SaveNotification(List<int> users, int SenderId, string Title, string? message, string url)
    {
        foreach (var UserId in users)
        {
            Notification notipayload = new Notification
            {
                UserId = UserId,
                SenderId = SenderId,
                Title = Title,
                Message = message,
                Url = url,
                IsRead = false,
                DateCreated = DateTime.UtcNow,
            };
            await _db.Notifications.AddAsync(notipayload);
            await _db.SaveChangesAsync();
        }
    }
}
