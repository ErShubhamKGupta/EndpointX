using EndpointX.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EndpointX.Controllers
{
    public static class IdentityUserEndpoints
    {
        public static IEndpointRouteBuilder MapIdentityUserEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPost("/signup", CreateUser);
            app.MapPost("/signin", SignIn);

            return app;
        }

        private static async Task<IResult> CreateUser(UserManager<ApplicationUser> userManager, [FromBody] UserRegistrationModel userRegistrationModel)
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

        }

        private static async Task<IResult> SignIn(UserManager<ApplicationUser> userManager, [FromBody] LoginModel loginModel, IOptions<AppSettings> appSettings)
        {
            var user = await userManager.FindByEmailAsync(loginModel.Email);
            if (user != null && await userManager.CheckPasswordAsync(user, loginModel.Password))
            {
                var signInKey = new SymmetricSecurityKey(
                                Encoding.UTF8.GetBytes(appSettings.Value.JWTSecretKey)
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
        }
    }
}
