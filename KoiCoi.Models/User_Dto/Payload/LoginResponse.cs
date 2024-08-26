namespace KoiCoi.Models.User_Dto.Payload;

public partial class LoginResponse
{
    public string LoginUserIdval { get; set; } = string.Empty;
    public string LoginName { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}
