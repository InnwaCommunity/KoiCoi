                                    KoiCoi
KoiCoi ဟာငွေကြေးစုဆောင်းမှုကို support ပေးသော software ဖြစ်ပါတယ်။

Features





```bash
dotnet ef dbcontext scaffold "Server=NYEINCHANNMOE;Database=KoiCoi;User Id=sa;Password=nyein@8834;TrustServerCertificate=True;" Microsoft.EntityFrameworkCore.SqlServer -o AppDbContextModels -c AppDbContext -f

Scaffold-DbContext "Server=NYEINCHANNMOE;Database=Koi_Coi;User ID=sa; Password=nyein@8834;Integrated Security=True;Trusted_Connection=true;TrustServerCertificate=True;" Microsoft.EntityFrameworkCore.SqlServer -OutputDir AppDbContext -Tables Tbl_AdminUserLogin -f
```

```bash
Encryption.EncryptID(item.UserId.ToString(), LoginUserId.ToString())
Convert.ToInt32(Encryption.DecryptID(id, LoginUserId.ToString()))
```

result pattern
```bash
Result<T>.Success(data);
Result<T>.Error(error);
```
eventpost
collectPost

Notification Url flow

Form/Details



                    Types
    if(don't have detail) From/null

    RequestChannelMember/channelMembershipId

    ActionByChannelAdminToJoinedMember/channelMembershipId ///show in channel member

    JoinedNewMember/channelMembershipId ///show in admins

    ApproveOrRejectByAdmin ///show in admins and User

    LeaveChannel/ ///show all members

    RemoveMember/             ///show all members

    UpcomingNewEvent/EventId ///show all members

    RejectedNewEvent/EventId ///show all channel admins and event creator

    RequestedNewEvent/EventId ///show all admins and owner

    ChannelUserTypeChange/membershipid ///show all admins and changed user

    EventUserTypeChange/membershipid ///show all admins and changed user

    RequestedNewCollectPost/postId ///show all event admins

    NewCollectPostAdded/postId ///show all channel members

    ApprovedOrRejectedPosts/postId ///show  login
    ApprovedOrRjectedPostToAdmin/postId ///show all member


Client မှာ UTC time ပဲပေးရမယ်


Approve Reject Status
Approve => 1
Reject  => 2

```
Globalfunction.StringToDecimal(_channel.TotalBalance == "0" || _channel.TotalBalance == null ? "0" : Encryption.DecryptID(_channel.TotalBalance.ToString(), balanceSalt))
```


DefaultPostPolicies

ViewPolicy -(1)
            MaxCount null
            StartDate - PostCreateDate
            EndDate - null
            GroupMemberOnly - true
            FriendOnly - false
ReactPolciy -(2)
            MaxCount null
            StartDate - PostCreateDate
            EndDate - Event EndDate
            GroupMemberOnly - true
            FriendOnly - false
Command -   (3)
            MaxCount null
            StartDate - PostCreateDate
            EndDate - Event EndDate
            GroupMemberOnly - true
            FriendOnly - false
Share -     (4)
            MaxCount null
            StartDate - PostCreateDate
            EndDate - null
            GroupMemberOnly - true
            FriendOnly - false

PostTypes(use lower case) - eventpost,collectpost,usagecollect