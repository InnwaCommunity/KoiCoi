namespace KoiCoi.Models.User_Dto.Payload;

public partial class SocialSignInPayload
{
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set;} = string.Empty;
    public string? Phone { get; set; }
    public string Password { get; set;} =  string.Empty;
    public string DeviceId { get; set; } = string.Empty;
    public string? FacebookUserId { get; set; }
    public string? GoogleUserId { get; set; }
}
