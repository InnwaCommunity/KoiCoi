
namespace KoiCoi.Operational.Services;
using Google.Apis.FirebaseCloudMessaging.v1;
using Google.Apis.FirebaseCloudMessaging.v1.Data;
public class FirebaseMessagingService
{
    private readonly string _projectId;
    private readonly FirebaseCloudMessagingService _fcmService;

    public FirebaseMessagingService(string projectId, FirebaseCloudMessagingService fcmService)
    {
        _projectId = projectId;
        _fcmService = fcmService;
    }

    public async Task SendNotificationAsync(string token, string title, string body)
    {
        var messageSend = new SendMessageRequest
        {
            Message = new Message
            {
                Token = token,
                Data = new Dictionary<string, string>
                {
                    { "title_message", title },
                    { "body_message", body }
                }
            }
        };

        var request = _fcmService.Projects.Messages.Send(messageSend, $"projects/{_projectId}");
        await request.ExecuteAsync();
    }

    public async Task SendNotificationToTopicAsync(string topic, string title, string body)
    {
        var message = new SendMessageRequest
        {
            Message = new Message
            {
                Topic = topic,
                Data = new Dictionary<string, string>
                {
                    { "title_message", title },
                    { "body_message", body }
                }
            }
        };

        var request = _fcmService.Projects.Messages.Send(message, $"projects/{_projectId}");
        await request.ExecuteAsync();
    }
}
