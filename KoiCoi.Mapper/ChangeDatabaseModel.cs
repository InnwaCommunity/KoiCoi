

namespace KoiCoi.Mapper;

public static class ChangeDatabaseModel
{
    #region User

    public static User ChangeUser(this ViaUser viaUser)
    {
        User user = new User
            {
                UserIdval = viaUser.UserIdval!,
                Name = viaUser.Name!,
                Email = string.IsNullOrWhiteSpace(viaUser.Email) ? null : viaUser.Email,
                Password = viaUser.Password!,
                Phone = viaUser.Phone,
                PasswordHash = viaUser.PasswordHash!,
                DeviceId = viaUser.DeviceId,
                DateCreated = DateTime.Now,
                ModifiedDate = DateTime.Now,
                Inactive = false,
        };
            return user;
    }

    public static Channel ChangeChannel(this ViaChannel viaChannel)
    {
        Channel channel = new Channel
        {
            ChannelName = viaChannel.ChannelName,
            StatusDescription = viaChannel.StatusDescription,
            ChannelType = viaChannel.ChannelType,
            CreatorId = viaChannel.CreatorId,
            CurrencyId = viaChannel.CurrencyId,
            MemberCount = 1,
            TotalBalance = "0",
            LastBalance = "0",
            DateCreated = DateTime.Now,
            ModifiedDate = DateTime.Now,
            Inactive = false
        };
        return channel;
    }
    public static ChannelProfile ChangeChannelProfile(this ViaChannelProfile viaChannelProfile)
    {
        ChannelProfile channelProfile = new ChannelProfile
        {
            Url = viaChannelProfile.Url,
            UrlDescription = viaChannelProfile.UrlDescription,
            ChannelId = viaChannelProfile.ChannelId,
            CreatedDate = DateTime.Now,
        };
        return channelProfile;
    }

    public static ChannelMembership ChangeChannMemberShip(this ViaChannelMemberShip viaChannelMemberShip)
    {
        ChannelMembership newChan = new ChannelMembership
        {
            ChannelId = viaChannelMemberShip.ChannelId,
            UserId = viaChannelMemberShip.UserId,
            UserTypeId = viaChannelMemberShip.UserTypeId,
            StatusId = viaChannelMemberShip.StatusId,
            JoinedDate = DateTime.Now,
        };
        return newChan;
    }

    public static ChannelType ChangeChannelType(this ViaChannelType viaChannelType)
    {
        ChannelType user = new ChannelType
        {
            ChannelTypeName = viaChannelType.ChannelTypeName ?? "",
            ChannelTypeDescription = viaChannelType.ChannelTypeDescription ?? ""
        };
        return user;
    }
    #endregion
}
