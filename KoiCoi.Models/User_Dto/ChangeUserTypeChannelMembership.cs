
namespace KoiCoi.Models.User_Dto;
public partial class ChangeUserTypeChannelMembership
{
    public string? ChannelIdval { get; set; }
    public List<UserIdAndUserType>? userIdAndUserTypes { get; set; }
}

public partial class UserIdAndUserType
{
    public string? UserIdval { get; set; }
    public string? UserTypeIdval { get; set; }
}

public partial class ChangeUserTypeEventMembership
{
    public string? EventIdval { get; set; }
    public List<UserIdAndUserType>? userIdAndUserTypes { get; set; }
}
