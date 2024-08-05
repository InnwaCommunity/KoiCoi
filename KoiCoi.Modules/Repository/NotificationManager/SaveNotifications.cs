namespace KoiCoi.Modules.Repository.NotificationManager;

public class SaveNotifications
{
    private readonly AppDbContext _db;

    public SaveNotifications(AppDbContext db)
    {
        _db = db;
    }

    public async Task<string> SaveNotification(List<int> users, int SenderId, string Title, string? message, string url)
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
        return "Success";
    }
}
