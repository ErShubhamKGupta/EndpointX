using EndpointX.Controllers;
using EndpointX.Extensions;
using EndpointX.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSwaggerExplorer()
                .InjectDbContext(builder.Configuration)
                .AddIdentityHandlersAndStores()
                .ConfigureIdentityOption()
                .AddIdentityAuth(builder.Configuration);

builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

var app = builder.Build();

app.ConfigureSwaggerExplorer()
    .AddIdentityAuthMiddleware();

app.MapControllers();
app.MapGroup("/api")
   .MapIdentityApi<ApplicationUser>();
app.MapGroup("/api")
    .MapIdentityUserEndpoints();

app.Run();
