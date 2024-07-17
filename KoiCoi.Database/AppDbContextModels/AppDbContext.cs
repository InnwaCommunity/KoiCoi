using System;
using System.Collections.Generic;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using Newtonsoft.Json.Linq;

namespace KoiCoi.Database.AppDbContextModels;

public partial class AppDbContext : DbContext
{
    private string _connectionString = "";
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
        var appsettingbuilder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
        var Configuration = appsettingbuilder.Build();

        _connectionString = Environment.GetEnvironmentVariable("DBSTRING") ?? Configuration.GetConnectionString("DefaultConnection")!;
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

    public virtual DbSet<PostCommand> PostCommands { get; set; }

    public virtual DbSet<PostImage> PostImages { get; set; }

    public virtual DbSet<PostPrivacy> PostPrivacies { get; set; }

    public virtual DbSet<PostShare> PostShares { get; set; }

    public virtual DbSet<React> Reacts { get; set; }

    public virtual DbSet<ReactType> ReactTypes { get; set; }

    public virtual DbSet<StatusType> StatusTypes { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserProfile> UserProfiles { get; set; }

    public virtual DbSet<UserType> UserTypes { get; set; }

    public dynamic RunExecuteQuery(string query, string queryparams)
    {
        var dal = new BaseDataAccess(_connectionString);
        return dal.ExecuteQuery(query, queryparams);
    }
    public async Task<dynamic> RunExecuteQueryAsync(string query, string queryparams)
    {
        var dal = new BaseDataAccess(_connectionString);
        return await dal.ExecuteQueryAsync(query, queryparams);
    }

    public dynamic RunExecuteRawQuery(string query, JObject queryparams)
    {
        var dal = new BaseDataAccess(_connectionString);
        return dal.ExecuteQueryNew(query, queryparams);
    }
    public dynamic RunExecuteReportQuery(string query)
    {
        var dal = new BaseDataAccess(_connectionString);
        return dal.ExecuteReportQuery(query);
    }

    public async Task<dynamic> RunExecuteRawQueryAsync(string query, JObject queryparams)
    {
        var dal = new BaseDataAccess(Database.GetDbConnection());
        return await dal.ExecuteQueryRawAsync(query, queryparams);
    }
    public int RunExecuteNonQuery(string query)
    {
        var dal = new BaseDataAccess(_connectionString);
        return dal.ExecuteNonQuery(query);
    }

    public int RunExecuteNonQueryWithParams(string query, JObject queryparams)
    {
        var dal = new BaseDataAccess(_connectionString);
        return dal.ExecuteNonQueryWithParams(query, queryparams);
    }

    public int RunExecuteQueryAndResponseEffectedCount(string query, JObject queryparams)
    {
        var dal = new BaseDataAccess(_connectionString);
        return dal.ExecuteQueryAndResponseEffectedCount(query, queryparams);
    }

    public async Task<T> GetAsync<T>(string command, object parms)
    {
        T result;
        using (MySqlConnection conn = new MySqlConnection(_connectionString))
        {
            result = (await conn.QueryAsync<T>(command, parms).ConfigureAwait(false)).FirstOrDefault();
        }
        return result;
    }

    public T GetSync<T>(string command, object parms)
    {
        T result;
        using (MySqlConnection conn = new MySqlConnection(_connectionString))
        {
            result = conn.Query<T>(command, parms).FirstOrDefault();
        }
        return result;
    }

    public async Task<IEnumerable<T>> GetListAsync<T>(string command, object parms, int? timeoutInSeconds = null)
    {
        using (MySqlConnection conn = new MySqlConnection(_connectionString))
        {
            await conn.OpenAsync().ConfigureAwait(false);

            var commandDefinition = new CommandDefinition(command, parms, commandTimeout: timeoutInSeconds);
            var result = await conn.QueryAsync<T>(commandDefinition).ConfigureAwait(false);
            return result;
        }
    }

    public IEnumerable<T> GetList<T>(string command, object parms, int? timeoutInSeconds = null)
    {
        using (MySqlConnection conn = new MySqlConnection(_connectionString))
        {
            conn.Open();

            var commandDefinition = new CommandDefinition(command, parms, commandTimeout: timeoutInSeconds);
            var result = conn.Query<T>(commandDefinition);
            return result;
        }
    }
    public int QuerySingle(string command, object parms)
    {
        int result = -1;
        using (MySqlConnection connection = new MySqlConnection(_connectionString))
        {
            connection.Open();
            result = connection.QuerySingle<int>(command, parms);
        }
        return result;
    }
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
            entity.HasKey(e => e.PostId).HasName("PK__Collect___5875F7AD26F33F2B");

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

        modelBuilder.Entity<PostCommand>(entity =>
        {
            entity.HasKey(e => e.CommandId).HasName("PK__PostComm__6B410B06CA45DDF2");

            entity.ToTable("PostCommand");

            entity.Property(e => e.CreatedDate)
                .HasColumnType("datetime")
                .HasColumnName("Created_Date");
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
            entity.HasKey(e => e.ShareId).HasName("PK__PostShar__D32A3FEE18ACCC78");

            entity.ToTable("PostShare");

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
            entity.HasKey(e => e.TypeId).HasName("PK__ReactTyp__516F03B539D74810");

            entity.Property(e => e.Description).HasMaxLength(50);
            entity.Property(e => e.Icon).HasMaxLength(30);
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
            entity.HasKey(e => e.UserId).HasName("PK__Users__B9BE370F11BFFDCB");

            entity.HasIndex(e => e.Email, "UQ__Users__AB6E616487CB97D3").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.DateCreated)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("date_created");
            entity.Property(e => e.DeviceId)
                .HasMaxLength(255)
                .HasColumnName("device_Id");
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
