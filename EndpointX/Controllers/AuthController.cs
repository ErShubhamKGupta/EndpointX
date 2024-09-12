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
        private readonly ILogger<AuthController> _logger;
        private readonly IConfiguration _config;

        public AuthController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ILogger<AuthController> logger, IConfiguration config)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
            _config = config;
        }

        [HttpPost]
        [Route("seed-roles")]
        public async Task<IActionResult> SeedRoles()
        {
            _logger.LogInformation("Started executing SeedRoles() method.");

            // Check for existing roles
            bool isOwnerRoleExists = await _roleManager.RoleExistsAsync(StaticUserRoles.SUPER_ADMIN);
            bool isAdminRoleExists = await _roleManager.RoleExistsAsync(StaticUserRoles.ADMIN);
            bool isUserRoleExists = await _roleManager.RoleExistsAsync(StaticUserRoles.USER);

            _logger.LogInformation("Role existence check completed. SUPER_ADMIN: {IsOwnerRoleExists}, ADMIN: {IsAdminRoleExists}, USER: {IsUserRoleExists}.",
                isOwnerRoleExists, isAdminRoleExists, isUserRoleExists);

            // If all roles already exist, log this and return a message
            if (isOwnerRoleExists && isAdminRoleExists && isUserRoleExists)
            {
                _logger.LogInformation("All required roles already exist. Skipping role seeding.");
                return Ok("Roles Seeding is Already Done");
            }

            // Create missing roles
            if (!isUserRoleExists)
            {
                await _roleManager.CreateAsync(new IdentityRole(StaticUserRoles.USER));
                _logger.LogInformation("Created USER role.");
            }

            if (!isAdminRoleExists)
            {
                await _roleManager.CreateAsync(new IdentityRole(StaticUserRoles.ADMIN));
                _logger.LogInformation("Created ADMIN role.");
            }

            if (!isOwnerRoleExists)
            {
                await _roleManager.CreateAsync(new IdentityRole(StaticUserRoles.SUPER_ADMIN));
                _logger.LogInformation("Created SUPER_ADMIN role.");
            }

            _logger.LogInformation("Roles seeding process completed successfully.");
            return Ok("Roles Seeding Succeeded.");
        }

        [HttpPost]
        [Route("register-user")]
        public async Task<IActionResult> Register([FromBody] UserRegistrationModel userRegistrationModel)
        {
            _logger.LogInformation("Started execution of registering a user with email: {Email}.", userRegistrationModel.Email);
            var isUserExist = await _userManager.FindByEmailAsync(userRegistrationModel.Email);

            if (isUserExist != null)
            {
                _logger.LogWarning("User with email {Email} is already registered.", userRegistrationModel.Email);
                return BadRequest($"User with the email {userRegistrationModel.Email} is already registered.");
            }

            ApplicationUser newUser = new ApplicationUser()
            {
                Email = userRegistrationModel.Email,
                FirstName = userRegistrationModel.FirstName,
                LastName = userRegistrationModel.LastName,
                DateOfBirth = DateTime.UtcNow,
                UserName = userRegistrationModel.Email
            };

            _logger.LogInformation("Creating a new user with email: {Email}.", userRegistrationModel.Email);
            var createdNewUser = await _userManager.CreateAsync(newUser, userRegistrationModel.Password);

            if (!createdNewUser.Succeeded)
            {
                _logger.LogError("Error occurred during user registration for email: {Email}.", userRegistrationModel.Email);
                var errorOccurred = $"There is an error occurred during user registration. Because: ";
                foreach (var error in createdNewUser.Errors)
                {
                    errorOccurred += $" # {error.Description}";
                }
                return BadRequest(errorOccurred);
            }

            await _userManager.AddToRoleAsync(newUser, StaticUserRoles.USER);
            _logger.LogInformation("User {FirstName} {LastName} with email {Email} has been assigned to the USER role.", newUser.FirstName, newUser.LastName, newUser.Email);
            return Ok($"New user registered successfully with the username {newUser.Email}.");

        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel loginModel, IOptions<AppSettings> appSettings)
        {
            _logger.LogInformation("Login attempt for user: {Email}", loginModel.Email);

            var user = await _userManager.FindByNameAsync(loginModel.Email);
            if (user == null)
            {
                _logger.LogWarning("Login failed: User with email {Email} not found.", loginModel.Email);
                return Unauthorized("Invalid Credentials");
            }

            var isPwdCorrect = await _userManager.CheckPasswordAsync(user, loginModel.Password);
            if (!isPwdCorrect)
            {
                _logger.LogWarning("Login failed: Incorrect password for user with email {Email}.", loginModel.Email);
                return Unauthorized("Invalid Credentials");
            }

            _logger.LogInformation("User {Email} successfully authenticated.", loginModel.Email);

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

            _logger.LogInformation($"Executing GenerateJwtToken() method to generate the JWT token");
            var token = GenerateJwtToken(authClaims, appSettings);
            _logger.LogInformation($"Token generated successfully: {token}");

            _logger.LogInformation("Token generated successfully for user {Email}.", loginModel.Email);
            return Ok(token);

        }

        private static string GenerateJwtToken(List<Claim> claims, IOptions<AppSettings>? appSettings)
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
