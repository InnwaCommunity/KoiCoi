namespace KoiCoi.Backend;

public static class ModularService
{
    public static WebApplicationBuilder AddSwagger(this WebApplicationBuilder builder)
    {
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
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
        return builder;
    }

    public static WebApplicationBuilder AddBusinessLogicService(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<BL_User>();
        return builder;
    }
}
