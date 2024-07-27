using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace KoiCoi.Database.AppDbContextModels;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Channel> Channels { get; set; }

    public virtual DbSet<ChannelBalanceRecord> ChannelBalanceRecords { get; set; }

    public virtual DbSet<ChannelMembership> ChannelMemberships { get; set; }

    public virtual DbSet<ChannelProfile> ChannelProfiles { get; set; }

    public virtual DbSet<ChannelType> ChannelTypes { get; set; }

    public virtual DbSet<CollectPost> CollectPosts { get; set; }

    public virtual DbSet<Currency> Currencies { get; set; }

    public virtual DbSet<Event> Events { get; set; }

    public virtual DbSet<EventBalanceRecord> EventBalanceRecords { get; set; }

    public virtual DbSet<EventImage> EventImages { get; set; }

    public virtual DbSet<EventMembership> EventMemberships { get; set; }

    public virtual DbSet<InformMail> InformMails { get; set; }

    public virtual DbSet<InviteHistory> InviteHistories { get; set; }

    public virtual DbSet<Otp> Otps { get; set; }

    public virtual DbSet<PostCommand> PostCommands { get; set; }

    public virtual DbSet<PostImage> PostImages { get; set; }

    public virtual DbSet<PostPrivacy> PostPrivacies { get; set; }

    public virtual DbSet<PostShare> PostShares { get; set; }

    public virtual DbSet<React> Reacts { get; set; }

    public virtual DbSet<ReactType> ReactTypes { get; set; }

    public virtual DbSet<RemoveMemberHistory> RemoveMemberHistories { get; set; }

    public virtual DbSet<StatusType> StatusTypes { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserProfile> UserProfiles { get; set; }

    public virtual DbSet<UserType> UserTypes { get; set; }

    public virtual DbSet<VisitChannelHistory> VisitChannelHistories { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=NYEINCHANNMOE;Database=Koi_Coi;User Id=sa;Password=nyein@8834;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Channel>(entity =>
        {
            entity.HasKey(e => e.ChannelId).HasName("PK__Channels__0548C1A0AACFB706");

            entity.Property(e => e.ChannelId).HasColumnName("Channel_Id");
            entity.Property(e => e.ChannelName)
                .HasMaxLength(30)
                .HasColumnName("Channel_Name");
            entity.Property(e => e.ChannelType).HasColumnName("Channel_Type");
            entity.Property(e => e.CreatorId).HasColumnName("Creator_id");
            entity.Property(e => e.CurrencyId).HasColumnName("Currency_id");
            entity.Property(e => e.DateCreated)
                .HasColumnType("datetime")
                .HasColumnName("date_created");
            entity.Property(e => e.Inactive)
                .HasDefaultValueSql("((0))")
                .HasColumnName("inactive");
            entity.Property(e => e.LastBalance)
                .HasMaxLength(200)
                .HasColumnName("Last_Balance");
            entity.Property(e => e.MemberCount).HasColumnName("Member_Count");
            entity.Property(e => e.ModifiedDate)
                .HasColumnType("datetime")
                .HasColumnName("modifiedDate");
            entity.Property(e => e.StatusDescription).HasColumnName("Status_Description");
            entity.Property(e => e.TotalBalance)
                .HasMaxLength(200)
                .HasColumnName("Total_Balance");
        });

        modelBuilder.Entity<ChannelBalanceRecord>(entity =>
        {
            entity.HasKey(e => e.BalanceRecordId).HasName("PK__ChannelB__7E7A3AA6AD2531FD");

            entity.ToTable("ChannelBalanceRecord");

            entity.Property(e => e.BalanceRecordId).HasColumnName("Balance_Record_Id");
            entity.Property(e => e.ChannelId).HasColumnName("Channel_Id");
            entity.Property(e => e.CreatedBalance)
                .HasColumnType("datetime")
                .HasColumnName("Created_Balance");
            entity.Property(e => e.LastBalance)
                .HasMaxLength(225)
                .HasColumnName("Last_Balance");
            entity.Property(e => e.TotalBalance)
                .HasMaxLength(225)
                .HasColumnName("Total_Balance");
        });

        modelBuilder.Entity<ChannelMembership>(entity =>
        {
            entity.HasKey(e => e.MembershipId).HasName("PK__ChannelM__0C6DBAA7E0426849");

            entity.ToTable("ChannelMembership");

            entity.Property(e => e.MembershipId).HasColumnName("Membership_Id");
            entity.Property(e => e.ChannelId).HasColumnName("Channel_Id");
            entity.Property(e => e.InviterId).HasColumnName("inviterId");
            entity.Property(e => e.JoinedDate)
                .HasColumnType("datetime")
                .HasColumnName("Joined_Date");
            entity.Property(e => e.UserId).HasColumnName("User_Id");
            entity.Property(e => e.UserTypeId).HasColumnName("UserType_Id");
        });

        modelBuilder.Entity<ChannelProfile>(entity =>
        {
            entity.HasKey(e => e.ProfileId).HasName("PK__Channel___A60ECB4281D05A08");

            entity.ToTable("Channel_Profiles");

            entity.Property(e => e.ProfileId).HasColumnName("Profile_Id");
            entity.Property(e => e.ChannelId).HasColumnName("Channel_Id");
            entity.Property(e => e.CreatedDate)
                .HasColumnType("datetime")
                .HasColumnName("Created_Date");
            entity.Property(e => e.Url).HasMaxLength(100);
            entity.Property(e => e.UrlDescription)
                .HasMaxLength(100)
                .HasColumnName("Url_Description");
        });

        modelBuilder.Entity<ChannelType>(entity =>
        {
            entity.HasKey(e => e.ChannelTypeId).HasName("PK__ChannelT__8192503D2EADEC50");

            entity.ToTable("ChannelType");

            entity.Property(e => e.ChannelTypeId).HasColumnName("Channel_Type_Id");
            entity.Property(e => e.ChannelTypeDescription)
                .HasMaxLength(225)
                .HasColumnName("Channel_Type_Description");
            entity.Property(e => e.ChannelTypeName)
                .HasMaxLength(50)
                .HasColumnName("Channel_Type_Name");
        });

        modelBuilder.Entity<CollectPost>(entity =>
        {
            entity.HasKey(e => e.PostId).HasName("PK__Collect___5875F7ADFD5BC384");

            entity.ToTable("Collect_Posts");

            entity.Property(e => e.PostId).HasColumnName("Post_Id");
            entity.Property(e => e.ApproverId).HasColumnName("Approver_Id");
            entity.Property(e => e.CollectAmount)
                .HasMaxLength(200)
                .HasColumnName("Collect_Amount");
            entity.Property(e => e.CreatedDate)
                .HasColumnType("datetime")
                .HasColumnName("Created_Date");
            entity.Property(e => e.CreaterId).HasColumnName("Creater_Id");
            entity.Property(e => e.EventId).HasColumnName("Event_Id");
            entity.Property(e => e.Inactive)
                .HasDefaultValueSql("((0))")
                .HasColumnName("inactive");
            entity.Property(e => e.ModifiedDate)
                .HasColumnType("datetime")
                .HasColumnName("Modified_Date");
            entity.Property(e => e.StatusId).HasColumnName("Status_Id");
        });

        modelBuilder.Entity<Currency>(entity =>
        {
            entity.HasKey(e => e.CurrencyId).HasName("PK__Currency__85A8F0C82B927B1A");

            entity.ToTable("Currency");

            entity.Property(e => e.CurrencyId).HasColumnName("Currency_Id");
            entity.Property(e => e.CurrencyName)
                .HasMaxLength(50)
                .HasColumnName("Currency_Name");
            entity.Property(e => e.CurrencySymbol)
                .HasMaxLength(15)
                .HasColumnName("Currency_Symbol");
            entity.Property(e => e.FractionalUnit)
                .HasMaxLength(30)
                .HasColumnName("Fractional_unit");
            entity.Property(e => e.IsoCode)
                .HasMaxLength(15)
                .HasColumnName("ISO_code");
        });

        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.Eventid).HasName("PK__Events__7945F468AC4D19F4");

            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.EndDate).HasColumnType("datetime");
            entity.Property(e => e.EventName).HasMaxLength(100);
            entity.Property(e => e.Inactive).HasDefaultValueSql("((0))");
            entity.Property(e => e.LastBalance).HasMaxLength(200);
            entity.Property(e => e.ModifiedDate).HasColumnType("datetime");
            entity.Property(e => e.StartDate).HasColumnType("datetime");
            entity.Property(e => e.TotalBalance).HasMaxLength(200);
        });

        modelBuilder.Entity<EventBalanceRecord>(entity =>
        {
            entity.HasKey(e => e.BalanceRecordId).HasName("PK__EventBal__7E7A3AA63019026A");

            entity.ToTable("EventBalanceRecord");

            entity.Property(e => e.BalanceRecordId).HasColumnName("Balance_Record_Id");
            entity.Property(e => e.CreatedDate)
                .HasColumnType("datetime")
                .HasColumnName("Created_Date");
            entity.Property(e => e.EventId).HasColumnName("Event_Id");
            entity.Property(e => e.LastBalance)
                .HasMaxLength(200)
                .HasColumnName("Last_Balance");
            entity.Property(e => e.TotalBalance)
                .HasMaxLength(200)
                .HasColumnName("Total_Balance");
        });

        modelBuilder.Entity<EventImage>(entity =>
        {
            entity.HasKey(e => e.UrlId).HasName("PK__Event_Im__A648537B1AC35B3D");

            entity.ToTable("Event_Image");

            entity.Property(e => e.UrlId).HasColumnName("Url_Id");
            entity.Property(e => e.CreatedDate)
                .HasColumnType("datetime")
                .HasColumnName("Created_Date");
            entity.Property(e => e.EventId).HasColumnName("Event_Id");
            entity.Property(e => e.ModifiedDate)
                .HasColumnType("datetime")
                .HasColumnName("Modified_Date");
            entity.Property(e => e.Url).HasMaxLength(100);
            entity.Property(e => e.UrlDescription)
                .HasMaxLength(100)
                .HasColumnName("Url_description");
        });

        modelBuilder.Entity<EventMembership>(entity =>
        {
            entity.HasKey(e => e.MembershipId).HasName("PK__Event_Me__0C6CB69F9503A6AF");

            entity.ToTable("Event_Membership");

            entity.Property(e => e.MembershipId).HasColumnName("Membership_id");
            entity.Property(e => e.EventId).HasColumnName("Event_Id");
            entity.Property(e => e.UserId).HasColumnName("User_Id");
            entity.Property(e => e.UserTypeId).HasColumnName("User_Type_Id");
        });

        modelBuilder.Entity<InformMail>(entity =>
        {
            entity.HasKey(e => e.MailId).HasName("PK__InformMa__09A8749A6D76A451");

            entity.Property(e => e.AppPassword).HasMaxLength(100);
            entity.Property(e => e.DateCreated)
                .HasColumnType("datetime")
                .HasColumnName("date_created");
            entity.Property(e => e.FromMail).HasMaxLength(100);
        });

        modelBuilder.Entity<InviteHistory>(entity =>
        {
            entity.HasKey(e => e.InviteId).HasName("PK__InviteHi__AFACE86DF041824C");

            entity.ToTable("InviteHistory");

            entity.Property(e => e.InviteData).HasMaxLength(100);
            entity.Property(e => e.JoinedDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<Otp>(entity =>
        {
            entity.HasKey(e => e.Otpid).HasName("PK__OTP__5C2EC48249FF1236");

            entity.ToTable("OTP");

            entity.Property(e => e.Otpid).HasColumnName("OTPId");
            entity.Property(e => e.CreatedDate)
                .HasColumnType("datetime")
                .HasColumnName("Created_Date");
            entity.Property(e => e.EmailPhone).HasMaxLength(100);
            entity.Property(e => e.Ipaddress)
                .HasMaxLength(100)
                .HasColumnName("IPAddress");
            entity.Property(e => e.LastModifiedDate).HasColumnType("datetime");
            entity.Property(e => e.Otptoken)
                .HasMaxLength(50)
                .HasColumnName("OTPToken");
            entity.Property(e => e.Passcode).HasMaxLength(30);
            entity.Property(e => e.SendDateTime).HasColumnType("datetime");
        });

        modelBuilder.Entity<PostCommand>(entity =>
        {
            entity.HasKey(e => e.CommandId).HasName("PK__PostComm__6B410B062B261047");

            entity.ToTable("PostCommand");

            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.ModifiedDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<PostImage>(entity =>
        {
            entity.HasKey(e => e.UrlId).HasName("PK__Post_Ima__A648537B6D74C9A6");

            entity.ToTable("Post_Images");

            entity.Property(e => e.UrlId).HasColumnName("Url_Id");
            entity.Property(e => e.CreatedDate)
                .HasColumnType("datetime")
                .HasColumnName("Created_Date");
            entity.Property(e => e.Description).HasMaxLength(100);
            entity.Property(e => e.PostId).HasColumnName("Post_Id");
            entity.Property(e => e.Url).HasMaxLength(100);
        });

        modelBuilder.Entity<PostPrivacy>(entity =>
        {
            entity.HasKey(e => e.PrivacyId).HasName("PK__PostPriv__1F7D0E970E2B12DE");

            entity.ToTable("PostPrivacy");

            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.Title).HasMaxLength(50);
        });

        modelBuilder.Entity<PostShare>(entity =>
        {
            entity.HasKey(e => e.ShareId).HasName("PK__PostShar__D32A3FEEAD64BF53");

            entity.ToTable("PostShare");

            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.Inactive)
                .HasDefaultValueSql("((0))")
                .HasColumnName("inactive");
            entity.Property(e => e.ModifiedDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<React>(entity =>
        {
            entity.HasKey(e => e.ReactId).HasName("PK__React__7661AD2F819FB25D");

            entity.ToTable("React");

            entity.Property(e => e.CreatedDate)
                .HasColumnType("datetime")
                .HasColumnName("Created_Date");
        });

        modelBuilder.Entity<ReactType>(entity =>
        {
            entity.HasKey(e => e.TypeId).HasName("PK__ReactTyp__516F03B539D74810");

            entity.Property(e => e.Description).HasMaxLength(50);
            entity.Property(e => e.Icon).HasMaxLength(30);
        });

        modelBuilder.Entity<RemoveMemberHistory>(entity =>
        {
            entity.HasKey(e => e.HistoryId).HasName("PK__RemoveMe__4D7B4ABD7DAA7A72");

            entity.ToTable("RemoveMemberHistory");

            entity.Property(e => e.Reason).HasMaxLength(100);
            entity.Property(e => e.RemoveDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<StatusType>(entity =>
        {
            entity.HasKey(e => e.StatusId).HasName("PK__StatusTy__5190094C7B90086A");

            entity.ToTable("StatusType");

            entity.Property(e => e.StatusId).HasColumnName("Status_Id");
            entity.Property(e => e.StatusDescription)
                .HasMaxLength(100)
                .HasColumnName("Status_Description");
            entity.Property(e => e.StatusName)
                .HasMaxLength(20)
                .HasColumnName("Status_Name");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__B9BE370FB4B5B396");

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.DateCreated)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("date_created");
            entity.Property(e => e.DeviceId)
                .HasMaxLength(100)
                .HasColumnName("device_Id");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.Inactive)
                .HasDefaultValueSql("((0))")
                .HasColumnName("inactive");
            entity.Property(e => e.ModifiedDate)
                .HasColumnType("datetime")
                .HasColumnName("modifiedDate");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.Password)
                .HasMaxLength(100)
                .HasColumnName("password");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(100)
                .HasColumnName("password_hash");
            entity.Property(e => e.Phone)
                .HasMaxLength(15)
                .HasColumnName("phone");
            entity.Property(e => e.UserIdval)
                .HasMaxLength(100)
                .HasColumnName("userIdval");
        });

        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.HasKey(e => e.ProfileId).HasName("PK__User_Pro__A60ECB42136B9F6C");

            entity.ToTable("User_Profiles");

            entity.Property(e => e.ProfileId).HasColumnName("Profile_Id");
            entity.Property(e => e.CreatedDate)
                .HasColumnType("datetime")
                .HasColumnName("Created_Date");
            entity.Property(e => e.Url).HasMaxLength(100);
            entity.Property(e => e.UrlDescription)
                .HasMaxLength(100)
                .HasColumnName("Url_Description");
            entity.Property(e => e.UserId).HasColumnName("User_Id");
        });

        modelBuilder.Entity<UserType>(entity =>
        {
            entity.HasKey(e => e.TypeId).HasName("PK__UserType__2C000598D9DA8FFA");

            entity.ToTable("UserType");

            entity.Property(e => e.TypeId).HasColumnName("type_id");
            entity.Property(e => e.Description)
                .HasMaxLength(100)
                .HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<VisitChannelHistory>(entity =>
        {
            entity.HasKey(e => e.HistoryId).HasName("PK__VisitCha__4D7B4ABDDF56FD00");

            entity.ToTable("VisitChannelHistory");

            entity.Property(e => e.ViewedDate).HasColumnType("datetime");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
