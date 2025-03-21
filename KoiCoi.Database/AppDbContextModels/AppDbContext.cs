﻿using System;
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

    public virtual DbSet<AccountLoginHistory> AccountLoginHistories { get; set; }

    public virtual DbSet<AddressType> AddressTypes { get; set; }

    public virtual DbSet<Channel> Channels { get; set; }

    public virtual DbSet<ChannelBalanceRecord> ChannelBalanceRecords { get; set; }

    public virtual DbSet<ChannelMarkBalance> ChannelMarkBalances { get; set; }

    public virtual DbSet<ChannelMembership> ChannelMemberships { get; set; }

    public virtual DbSet<ChannelProfile> ChannelProfiles { get; set; }

    public virtual DbSet<ChannelTopic> ChannelTopics { get; set; }

    public virtual DbSet<ChannelType> ChannelTypes { get; set; }

    public virtual DbSet<CollectPost> CollectPosts { get; set; }

    public virtual DbSet<CommandReact> CommandReacts { get; set; }

    public virtual DbSet<CommandViewer> CommandViewers { get; set; }

    public virtual DbSet<Event> Events { get; set; }

    public virtual DbSet<EventAddress> EventAddresses { get; set; }

    public virtual DbSet<EventAllowedMark> EventAllowedMarks { get; set; }

    public virtual DbSet<EventBalanceRecord> EventBalanceRecords { get; set; }

    public virtual DbSet<EventFile> EventFiles { get; set; }

    public virtual DbSet<EventMarkBalance> EventMarkBalances { get; set; }

    public virtual DbSet<EventMembership> EventMemberships { get; set; }

    public virtual DbSet<EventTag> EventTags { get; set; }

    public virtual DbSet<ExchangeRate> ExchangeRates { get; set; }

    public virtual DbSet<InformMail> InformMails { get; set; }

    public virtual DbSet<InviteHistory> InviteHistories { get; set; }

    public virtual DbSet<Mark> Marks { get; set; }

    public virtual DbSet<MarkType> MarkTypes { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<NotificationToken> NotificationTokens { get; set; }

    public virtual DbSet<Otp> Otps { get; set; }

    public virtual DbSet<Policy> Policies { get; set; }

    public virtual DbSet<Post> Posts { get; set; }

    public virtual DbSet<PostBalance> PostBalances { get; set; }

    public virtual DbSet<PostCommand> PostCommands { get; set; }

    public virtual DbSet<PostImage> PostImages { get; set; }

    public virtual DbSet<PostPolicyProperty> PostPolicyProperties { get; set; }

    public virtual DbSet<PostShare> PostShares { get; set; }

    public virtual DbSet<PostTag> PostTags { get; set; }

    public virtual DbSet<PostViewer> PostViewers { get; set; }

    public virtual DbSet<React> Reacts { get; set; }

    public virtual DbSet<ReactType> ReactTypes { get; set; }

    public virtual DbSet<RemoveMemberHistory> RemoveMemberHistories { get; set; }

    public virtual DbSet<StatusType> StatusTypes { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserPostInteraction> UserPostInteractions { get; set; }

    public virtual DbSet<UserProfile> UserProfiles { get; set; }

    public virtual DbSet<UserType> UserTypes { get; set; }

    public virtual DbSet<VisitChannelHistory> VisitChannelHistories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AccountLoginHistory>(entity =>
        {
            entity.HasKey(e => e.HistoryId).HasName("PK__AccountL__4D7B4ABD320026D0");

            entity.ToTable("AccountLoginHistory");

            entity.Property(e => e.AppVersion).HasMaxLength(30);
            entity.Property(e => e.CreatedData).HasColumnType("datetime");
            entity.Property(e => e.DeviceId).HasMaxLength(30);
            entity.Property(e => e.ModifiedData).HasColumnType("datetime");
            entity.Property(e => e.OsVersion).HasMaxLength(30);
            entity.Property(e => e.PhoneModel).HasMaxLength(30);
        });

        modelBuilder.Entity<AddressType>(entity =>
        {
            entity.HasKey(e => e.AddressId).HasName("PK__AddressT__091C2AFB8E1652D9");

            entity.ToTable("AddressType");

            entity.Property(e => e.Address).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(100);
        });

        modelBuilder.Entity<Channel>(entity =>
        {
            entity.HasKey(e => e.ChannelId).HasName("PK__Channels__0548C1A0701CB1CC");

            entity.Property(e => e.ChannelId).HasColumnName("Channel_Id");
            entity.Property(e => e.ChannelName)
                .HasMaxLength(30)
                .HasColumnName("Channel_Name");
            entity.Property(e => e.ChannelType).HasColumnName("Channel_Type");
            entity.Property(e => e.CreatorId).HasColumnName("Creator_id");
            entity.Property(e => e.DateCreated)
                .HasColumnType("datetime")
                .HasColumnName("date_created");
            entity.Property(e => e.Inactive)
                .HasDefaultValueSql("((0))")
                .HasColumnName("inactive");
            entity.Property(e => e.MemberCount).HasColumnName("Member_Count");
            entity.Property(e => e.ModifiedDate)
                .HasColumnType("datetime")
                .HasColumnName("modifiedDate");
            entity.Property(e => e.StatusDescription).HasColumnName("Status_Description");
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

        modelBuilder.Entity<ChannelMarkBalance>(entity =>
        {
            entity.HasKey(e => e.BalanceId).HasName("PK__ChannelM__A760D5BE6BC90E5A");

            entity.Property(e => e.LastBalance)
                .HasMaxLength(255)
                .HasColumnName("Last_Balance");
            entity.Property(e => e.TotalBalance)
                .HasMaxLength(255)
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

        modelBuilder.Entity<ChannelTopic>(entity =>
        {
            entity.HasKey(e => e.TopicId).HasName("PK__ChannelT__022E0F5DCA02A51F");

            entity.ToTable("ChannelTopic");

            entity.Property(e => e.DateCreated)
                .HasColumnType("datetime")
                .HasColumnName("date_created");
            entity.Property(e => e.Descriptions).HasMaxLength(100);
            entity.Property(e => e.TopicName).HasMaxLength(100);
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
            entity.HasKey(e => e.CollectId).HasName("PK__CollectP__8AAA9E0A4C6DE3DE");

            entity.ToTable("CollectPost");
        });

        modelBuilder.Entity<CommandReact>(entity =>
        {
            entity.HasKey(e => e.ReactId).HasName("PK__CommandR__7661AD2F06907B4E");

            entity.ToTable("CommandReact");

            entity.Property(e => e.CreatedDate)
                .HasColumnType("datetime")
                .HasColumnName("Created_Date");
        });

        modelBuilder.Entity<CommandViewer>(entity =>
        {
            entity.HasKey(e => e.ViewId).HasName("PK__CommandV__1E371CF69E3DCD8D");

            entity.ToTable("CommandViewer");

            entity.Property(e => e.CreatedDate)
                .HasColumnType("datetime")
                .HasColumnName("Created_Date");
        });

        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.Eventid).HasName("PK__Events__7945F46808A9A89B");

            entity.Property(e => e.EndDate).HasColumnType("datetime");
            entity.Property(e => e.EventName).HasMaxLength(100);
            entity.Property(e => e.StartDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<EventAddress>(entity =>
        {
            entity.HasKey(e => e.EventAddressId).HasName("PK__EventAdd__AAD5E9C2F7F26E2E");

            entity.ToTable("EventAddress");

            entity.Property(e => e.AddressName).HasMaxLength(255);
        });

        modelBuilder.Entity<EventAllowedMark>(entity =>
        {
            entity.HasKey(e => e.AllowedMarkId).HasName("PK__EventAll__959F221805029BA7");

            entity.ToTable("EventAllowedMark");

            entity.Property(e => e.AllowedMarkName).HasMaxLength(200);
        });

        modelBuilder.Entity<EventBalanceRecord>(entity =>
        {
            entity.HasKey(e => e.BalanceRecordId).HasName("PK__EventBal__7E7A3AA63019026A");

            entity.ToTable("EventBalanceRecord");

            entity.Property(e => e.BalanceRecordId).HasColumnName("Balance_Record_Id");
            entity.Property(e => e.CreatedDate)
                .HasColumnType("datetime")
                .HasColumnName("Created_Date");
            entity.Property(e => e.LastBalance)
                .HasMaxLength(200)
                .HasColumnName("Last_Balance");
            entity.Property(e => e.TotalBalance)
                .HasMaxLength(200)
                .HasColumnName("Total_Balance");
        });

        modelBuilder.Entity<EventFile>(entity =>
        {
            entity.HasKey(e => e.UrlId).HasName("PK__EventFil__A648537BD1C1FACF");

            entity.Property(e => e.UrlId).HasColumnName("Url_Id");
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.Extension).HasMaxLength(30);
            entity.Property(e => e.ModifiedDate).HasColumnType("datetime");
            entity.Property(e => e.Url).HasMaxLength(100);
            entity.Property(e => e.UrlDescription)
                .HasMaxLength(100)
                .HasColumnName("Url_description");
        });

        modelBuilder.Entity<EventMarkBalance>(entity =>
        {
            entity.HasKey(e => e.BalanceId).HasName("PK__EventMar__A760D5BEEE7A9DF7");

            entity.Property(e => e.LastBalance).HasMaxLength(255);
            entity.Property(e => e.TargetBalance).HasMaxLength(255);
            entity.Property(e => e.TotalBalance).HasMaxLength(255);
        });

        modelBuilder.Entity<EventMembership>(entity =>
        {
            entity.HasKey(e => e.Membershipid).HasName("PK__Event_Me__0C6CB69F9503A6AF");

            entity.ToTable("EventMembership");
        });

        modelBuilder.Entity<EventTag>(entity =>
        {
            entity.HasKey(e => e.EventTagId).HasName("PK__EventTag__1DC94B514BA9F5A2");

            entity.ToTable("EventTag");

            entity.Property(e => e.CreateDate).HasColumnType("datetime");
            entity.Property(e => e.TagDescription).HasMaxLength(255);
            entity.Property(e => e.TagName).HasMaxLength(50);
        });

        modelBuilder.Entity<ExchangeRate>(entity =>
        {
            entity.HasKey(e => e.ExchangeRateId).HasName("PK__Exchange__B0560449DE4DA664");

            entity.ToTable("ExchangeRate");

            entity.Property(e => e.MinQuantity).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Rate).HasColumnType("decimal(18, 6)");

            entity.HasOne(d => d.EventPost).WithMany(p => p.ExchangeRates)
                .HasForeignKey(d => d.EventPostId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ExchangeRate_EventPost");

            entity.HasOne(d => d.FromMark).WithMany(p => p.ExchangeRateFromMarks)
                .HasForeignKey(d => d.FromMarkId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ExchangeRate_FromMark");

            entity.HasOne(d => d.ToMark).WithMany(p => p.ExchangeRateToMarks)
                .HasForeignKey(d => d.ToMarkId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ExchangeRate_ToMark");
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

        modelBuilder.Entity<Mark>(entity =>
        {
            entity.HasKey(e => e.MarkId).HasName("PK__Mark__4E30D366F2649C53");

            entity.ToTable("Mark");

            entity.Property(e => e.Isocode)
                .HasMaxLength(15)
                .HasColumnName("ISOcode");
            entity.Property(e => e.MarkName).HasMaxLength(50);
            entity.Property(e => e.MarkSymbol).HasMaxLength(50);

            entity.HasOne(d => d.Channel).WithMany(p => p.Marks)
                .HasForeignKey(d => d.ChannelId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Mark_Channel");

            entity.HasOne(d => d.User).WithMany(p => p.Marks)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Mark_User");
        });

        modelBuilder.Entity<MarkType>(entity =>
        {
            entity.HasKey(e => e.MarkTypeId).HasName("PK__MarkType__1C224AF691E27537");

            entity.ToTable("MarkType");

            entity.Property(e => e.TypeDescription).HasMaxLength(100);
            entity.Property(e => e.TypeName).HasMaxLength(50);
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("PK__Notifica__20CF2E121AB1EC56");

            entity.Property(e => e.DateCreated)
                .HasColumnType("datetime")
                .HasColumnName("date_created");
            entity.Property(e => e.IsRead).HasDefaultValueSql("((0))");
            entity.Property(e => e.Title).HasMaxLength(100);
            entity.Property(e => e.Url).HasMaxLength(100);
        });

        modelBuilder.Entity<NotificationToken>(entity =>
        {
            entity.HasKey(e => e.TokenId).HasName("PK__Notifica__658FEEEAED11443F");

            entity.ToTable("NotificationToken");

            entity.Property(e => e.AppVersion).HasMaxLength(30);
            entity.Property(e => e.IsRooted).HasDefaultValueSql("((0))");
            entity.Property(e => e.LastActivities).HasColumnType("datetime");
            entity.Property(e => e.OsVersion).HasMaxLength(30);
            entity.Property(e => e.PhModel).HasMaxLength(50);
            entity.Property(e => e.Token).HasMaxLength(255);
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

        modelBuilder.Entity<Policy>(entity =>
        {
            entity.HasKey(e => e.PolicyId).HasName("PK__Policy__2E1339A41B48E019");

            entity.ToTable("Policy");

            entity.Property(e => e.Description).HasMaxLength(100);
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<Post>(entity =>
        {
            entity.HasKey(e => e.PostId).HasName("PK__Post__AA126018815EAB73");

            entity.ToTable("Post");

            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.ModifiedDate).HasColumnType("datetime");
            entity.Property(e => e.PostType).HasMaxLength(50);
        });

        modelBuilder.Entity<PostBalance>(entity =>
        {
            entity.HasKey(e => e.BalanceId).HasName("PK__PostBala__A760D5BE6F0E7A7D");

            entity.ToTable("PostBalance");

            entity.Property(e => e.Balance).HasMaxLength(255);
        });

        modelBuilder.Entity<PostCommand>(entity =>
        {
            entity.HasKey(e => e.CommandId).HasName("PK__PostComm__6B410B06CE99E604");

            entity.ToTable("PostCommand");

            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.ModifiedDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<PostImage>(entity =>
        {
            entity.HasKey(e => e.ImageId).HasName("PK__PostImag__7516F70CD68E0D6C");

            entity.Property(e => e.CreatedDate)
                .HasColumnType("datetime")
                .HasColumnName("Created_Date");
            entity.Property(e => e.Description).HasMaxLength(100);
            entity.Property(e => e.Url).HasMaxLength(100);
        });

        modelBuilder.Entity<PostPolicyProperty>(entity =>
        {
            entity.HasKey(e => e.PropertyId).HasName("PK__PostPoli__70C9A73587DAF83E");

            entity.Property(e => e.EndDate).HasColumnType("datetime");
            entity.Property(e => e.StartDate).HasColumnType("datetime");
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

        modelBuilder.Entity<PostTag>(entity =>
        {
            entity.ToTable("PostTag");
        });

        modelBuilder.Entity<PostViewer>(entity =>
        {
            entity.HasKey(e => e.ViewerId).HasName("PK__PostView__6DA5554DDE318377");

            entity.ToTable("PostViewer");

            entity.Property(e => e.CreatedDate)
                .HasColumnType("datetime")
                .HasColumnName("Created_Date");
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
            entity.HasKey(e => e.TypeId).HasName("PK__ReactTyp__516F03B5651AE0A9");

            entity.Property(e => e.Description).HasMaxLength(50);
            entity.Property(e => e.Emoji).HasMaxLength(100);
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
            entity.Property(e => e.FacebookUserId).HasMaxLength(100);
            entity.Property(e => e.GoogleUserId).HasMaxLength(100);
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

        modelBuilder.Entity<UserPostInteraction>(entity =>
        {
            entity.HasKey(e => e.InteractionId).HasName("PK__UserPost__922C049633BC375D");

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime")
                .HasColumnName("Created_Date");
            entity.Property(e => e.PostType).HasMaxLength(50);
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime")
                .HasColumnName("Updated_Date");
            entity.Property(e => e.ViewedContext).HasMaxLength(50);
            entity.Property(e => e.VisibilityPercentage).HasColumnType("decimal(5, 2)");
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
