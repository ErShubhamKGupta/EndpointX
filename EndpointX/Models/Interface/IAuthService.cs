using EndpointX.Models.DTO;

namespace EndpointX.Models.Interface
{
    public interface IAuthService
    {
        Task<AuthServiceResponseDto> SeedRolesAsync();
        Task<AuthServiceResponseDto> RegisterAsync(UserRegistrationModel registerDto);
        Task<AuthServiceResponseDto> LoginAsync(LoginModel loginDto);
        Task<AuthServiceResponseDto> MakeAdminAsync(UpdatePermissionDto updatePermissionDto);
        Task<AuthServiceResponseDto> MakeOwnerAsync(UpdatePermissionDto updatePermissionDto);
    }
}
