using EndpointX.Models;
using EndpointX.Models.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configure database context to use SQL Server
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme =
    x.DefaultChallengeScheme =
    x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(y =>
{
    y.SaveToken = false;
    y.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    builder.Configuration["AppSettings:JWTSecretKey"]!))
    };
});

builder.Services.AddAuthorization();
builder.Services.AddIdentityApiEndpoints<ApplicationUser>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.User.RequireUniqueEmail = true;
});



// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app
    .MapGroup("/api")
    .MapIdentityApi<ApplicationUser>();


app.MapControllers();

app.MapPost("/api/signup", async (
    UserManager<ApplicationUser> userManager,
    [FromBody] UserRegistrationModel userRegistrationModel
    ) =>
{
    ApplicationUser user = new ApplicationUser()
    {
        UserName = userRegistrationModel.Email,
        Email = userRegistrationModel.Email,
        FirstName = userRegistrationModel.FirstName,
        LastName = userRegistrationModel.LastName,
    };
    var result = await userManager.CreateAsync(
        user,
        userRegistrationModel.Password);

    if (result.Succeeded)
        return Results.Ok(result);
    else
        return Results.BadRequest(result);
});

app.MapPost("/api/signin", async (
    UserManager<ApplicationUser> userManager,
    [FromBody] LoginModel loginModel) =>
{
    var user = await userManager.FindByEmailAsync(loginModel.Email);
    if (user != null && await userManager.CheckPasswordAsync(user, loginModel.Password))
    {
        var signInKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(builder.Configuration["AppSettings:JWTSecretKey"]!)
                        );
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
            [
                new("UserID",user.Id.ToString())
            ]),
            Expires = DateTime.UtcNow.AddMinutes(30),
            SigningCredentials = new SigningCredentials(
                signInKey,
                SecurityAlgorithms.HmacSha256Signature
                )
        };
        var tokenHandler = new JwtSecurityTokenHandler();
        var securityToken = tokenHandler.CreateToken(tokenDescriptor);
        var token = tokenHandler.WriteToken(securityToken);
        return Results.Ok(new { token });
    }
    else
        return Results.BadRequest(new { message = "Username or password is incorrect." });
});

app.Run();
