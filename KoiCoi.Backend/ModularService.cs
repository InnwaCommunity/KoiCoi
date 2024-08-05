using Google.Apis.Auth.OAuth2;
using Google.Apis.FirebaseCloudMessaging.v1;
using Google.Apis.Services;
using KoiCoi.Backend.CustomTokenAuthProvider;
using KoiCoi.Operational.Services;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.OpenApi.Models;

namespace KoiCoi.Backend;

public static class ModularService
{
    public static WebApplicationBuilder AddSwagger(this WebApplicationBuilder builder)
    {
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Koi Coi", Version = "v1" });
        });
        builder.Services.AddTransient<TokenProviderMiddleware>();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        builder.Services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

        builder.Services.AddMvc(option => option.EnableEndpointRouting = false)
         .AddNewtonsoftJson(o =>
         {
             o.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
             o.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Include;
             o.SerializerSettings.DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Include;    //it must be Include, otherwise default value (boolean=false, int=0, int?=null, object=null) will be missing in response json			
             o.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver();
         });

        builder.Services.AddControllers();


        builder.Services.AddSession(options =>   //use session for carry event log data like login, ip, etc.
        {
            options.IdleTimeout = TimeSpan.FromSeconds(30);
        });

        builder.Services.Configure<KestrelServerOptions>(options =>
        {
            options.AllowSynchronousIO = true;
        });
        return builder;
    }

    public static WebApplicationBuilder AddDbService(this WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<AppDbContext>(opt =>
        {
            opt.UseSqlServer(builder.Configuration.GetConnectionString("DbConnection"));
        }, ServiceLifetime.Transient, ServiceLifetime.Transient);
        return builder;
    }

    public static WebApplicationBuilder AddDataAccessService(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<DA_User>();
        builder.Services.AddScoped<DA_ChangePassword>();
        builder.Services.AddScoped<DA_Channel>();
        builder.Services.AddScoped<DA_Event>();
        builder.Services.AddScoped<DA_Post>();
        builder.Services.AddScoped<SaveNotifications>();
        return builder;
    }

    public static WebApplicationBuilder SetupFirebase(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<FirebaseMessagingService>(serviceProvider =>
        {
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();

            var projectId = configuration["Firebase:ProjectId"];
            var credential = GoogleCredential.FromFile("serviceaccountkey.json");
            var fcmService = new FirebaseCloudMessagingService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential
            });
            return new FirebaseMessagingService(projectId!, fcmService);
        });
        return builder;
    }

    public static WebApplicationBuilder AddBusinessLogicService(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<BL_User>();
        builder.Services.AddScoped<BL_ChangePassword>();
        builder.Services.AddScoped<BL_Channel>();
        builder.Services.AddScoped<BL_Event>();
        builder.Services.AddScoped<BL_Post>();
        return builder;
    }

    public static WebApplicationBuilder ConfigureCors(this WebApplicationBuilder builder)
    {
        var corsBuilder = new CorsPolicyBuilder();
        corsBuilder.AllowAnyHeader();
        corsBuilder.WithMethods("GET", "POST", "PUT", "DELETE");
        corsBuilder.WithOrigins((builder.Configuration["AllowedOrigins"] ?? "http://localhost").Split(","));
        corsBuilder.AllowCredentials();

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("CorsAllowAllPolicy", corsBuilder.Build());
        });
        return builder;
    }
    //it is for error handler of model validation exception when direct bind request parameter to model in controller function
    public static WebApplicationBuilder ConfigureModelBindingExceptionHandling(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = actionContext =>
            {
                ValidationProblemDetails error = actionContext.ModelState
                      .Where(e => e.Value is not null && e.Value.Errors.Count > 0)
                      .Select(e => new ValidationProblemDetails(actionContext.ModelState)).First();

                string ErrorMessage = "";
                foreach (KeyValuePair<string, string[]> errobj in error.Errors)
                {
                    foreach (string s in errobj.Value)
                    {
                        ErrorMessage = ErrorMessage + s + "\r\n";
                    }
                }
                return new BadRequestObjectResult(new { data = 0, error = ErrorMessage });
            };
        });
        return builder;
    }
}
