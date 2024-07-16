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

    public virtual DbSet<PostImage> PostImages { get; set; }

    public virtual DbSet<StatusType> StatusTypes { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserProfile> UserProfiles { get; set; }

    public virtual DbSet<UserType> UserTypes { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=NYEINCHANNMOE;Database=Koi_Coi;User Id=sa;Password=nyein@8834;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Channel>(entity =>
        {
            entity.HasKey(e => e.ChannelId).HasName("PK__Channels__0548C1A0463CBB45");

            entity.Property(e => e.ChannelId).HasColumnName("Channel_Id");
            entity.Property(e => e.ChannelName)
                .HasMaxLength(30)
                .HasColumnName("Channel_Name");
            entity.Property(e => e.ChannelType).HasColumnName("Channel_Type");
            entity.Property(e => e.CreatedDate)
                .HasColumnType("datetime")
                .HasColumnName("Created_Date");
            entity.Property(e => e.CreatorId).HasColumnName("Creator_id");
            entity.Property(e => e.CurrencyId).HasColumnName("Currency_id");
            entity.Property(e => e.LastBalance)
                .HasMaxLength(200)
                .HasColumnName("Last_Balance");
            entity.Property(e => e.MemberCount).HasColumnName("Member_Count");
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
            entity.HasKey(e => e.MembershipId).HasName("PK__ChannelM__0C6DBAA798BD6952");

            entity.ToTable("ChannelMembership");

            entity.Property(e => e.MembershipId).HasColumnName("Membership_Id");
            entity.Property(e => e.ChannelId).HasColumnName("Channel_Id");
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
            entity.HasKey(e => e.PostId).HasName("PK__Collect___5875F7AD9AB665F5");

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
            entity.HasKey(e => e.EventId).HasName("PK__Events__FD6AEB9CB940A555");

            entity.Property(e => e.EventId).HasColumnName("Event_id");
            entity.Property(e => e.ApproverId).HasColumnName("Approver_Id");
            entity.Property(e => e.ChannelId).HasColumnName("Channel_Id");
            entity.Property(e => e.CreatedDate)
                .HasColumnType("datetime")
                .HasColumnName("Created_Date");
            entity.Property(e => e.CreatorId).HasColumnName("Creator_id");
            entity.Property(e => e.EndDate)
                .HasColumnType("datetime")
                .HasColumnName("End_Date");
            entity.Property(e => e.EventDescription).HasColumnName("Event_description");
            entity.Property(e => e.EventName)
                .HasMaxLength(100)
                .HasColumnName("Event_name");
            entity.Property(e => e.LastBalance)
                .HasMaxLength(200)
                .HasColumnName("Last_Balance");
            entity.Property(e => e.StartDate)
                .HasColumnType("datetime")
                .HasColumnName("Start_Date");
            entity.Property(e => e.StatusId).HasColumnName("Status_Id");
            entity.Property(e => e.TotalBalance)
                .HasMaxLength(200)
                .HasColumnName("Total_Balance");
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
            entity.HasKey(e => e.UserId).HasName("PK__Users__B9BE370FFA5AB70B");

            entity.HasIndex(e => e.Email, "UQ__Users__AB6E6164ED323E83").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.DateCreated)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("date_created");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.Password)
                .HasMaxLength(100)
                .HasColumnName("password");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasColumnName("password_hash");
            entity.Property(e => e.Phone)
                .HasMaxLength(15)
                .HasColumnName("phone");
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

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
