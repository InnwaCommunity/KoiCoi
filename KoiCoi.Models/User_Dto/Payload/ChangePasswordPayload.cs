namespace KoiCoi.Models.User_Dto.Payload;
public class ChangePasswordPayload
{
    public string OldPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
