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
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _config;

        public AuthController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration config)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _config = config;
        }

        [HttpPost]
        [Route("seed-roles")]
        public async Task<IActionResult> SeedRoles()
        {
            bool isOwnerRoleExists = await _roleManager.RoleExistsAsync(StaticUserRoles.SUPER_ADMIN);
            bool isAdminRoleExists = await _roleManager.RoleExistsAsync(StaticUserRoles.ADMIN);
            bool isUserRoleExists = await _roleManager.RoleExistsAsync(StaticUserRoles.USER);

            if (isOwnerRoleExists && isAdminRoleExists && isUserRoleExists)
                return Ok("Roles Seeding is Already Done");

            await _roleManager.CreateAsync(new IdentityRole(StaticUserRoles.USER));
            await _roleManager.CreateAsync(new IdentityRole(StaticUserRoles.ADMIN));
            await _roleManager.CreateAsync(new IdentityRole(StaticUserRoles.SUPER_ADMIN));

            return Ok("Roles Seeding Succeeded.");
        }

        [HttpPost]
        [Route("register-user")]
        public async Task<IActionResult> Register([FromBody] UserRegistrationModel userRegistrationModel)
        {
            var isUserExist = await _userManager.FindByEmailAsync(userRegistrationModel.Email);

            if (isUserExist != null) return BadRequest($"User with the email {userRegistrationModel.Email} is already registered.");

            ApplicationUser newUser = new ApplicationUser()
            {
                Email = userRegistrationModel.Email,
                FirstName = userRegistrationModel.FirstName,
                LastName = userRegistrationModel.LastName,
                DateOfBirth = DateTime.UtcNow,
                UserName = userRegistrationModel.Email
            };

            var createdNewUser = await _userManager.CreateAsync(newUser, userRegistrationModel.Password);

            if (!createdNewUser.Succeeded)
            {
                var errorOccurred = $"There is an error occurred during user registration. Because: ";
                foreach (var error in createdNewUser.Errors)
                {
                    errorOccurred += $" # {error.Description}";
                }
                return BadRequest(errorOccurred);
            }

            await _userManager.AddToRoleAsync(newUser, StaticUserRoles.USER);
            return Ok($"New user registered successfully with the username {newUser.Email}.");

        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel loginModel, IOptions<AppSettings> appSettings)
        {
            var user = await _userManager.FindByNameAsync(loginModel.Email);
            if (user == null) return Unauthorized($"Invalid Credentials");

            var isPwdCorrect = await _userManager.CheckPasswordAsync(user, loginModel.Password);
            if (!isPwdCorrect) return Unauthorized($"Invalid Credentials");

            var userRoles = await _userManager.GetRolesAsync(user);
            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name,user.UserName),
                new Claim(ClaimTypes.NameIdentifier,user.Id),
                new Claim ("JWTID",Guid.NewGuid().ToString()),
            };

            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }

            var token = GenerateJwtToken(authClaims, appSettings);

            return Ok(token);

        }

        private string GenerateJwtToken(List<Claim> claims, IOptions<AppSettings>? appSettings)
        {
            var signInKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(appSettings.Value.JWTSecretKey));

            var tokenObj = new JwtSecurityToken(
                     issuer: appSettings.Value.Issuer,
                     audience: appSettings.Value.Audience,
                     claims: claims,
                     expires: DateTime.UtcNow.AddMinutes(30),
                     signingCredentials: new SigningCredentials(signInKey, SecurityAlgorithms.HmacSha256Signature)

                );
            string token = new JwtSecurityTokenHandler().WriteToken(tokenObj);
            return token;

        }


    }
}
