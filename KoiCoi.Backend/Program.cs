
using KoiCoi.Backend.CustomTokenAuthProvider;
using Amazon.Lambda.AspNetCoreServer.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder
    .AddSwagger()
    .AddDbService()
    .AddDataAccessService()
    .AddBusinessLogicService()
    .ConfigureCors()
    .ConfigureModelBindingExceptionHandling();

builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

var app = builder.Build();
//if (app.Environment.IsDevelopment())
//{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Happy Cooky v1"));
//}

app.MapGet("/test", () => "Hello World!");
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.UseSession();

app.UseCors("CorsAllowAllPolicy");

app.UseRouting();

app.UseTokenProviderMiddleware();


app.Run();

 
