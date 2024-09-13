using System.ComponentModel.DataAnnotations;

namespace EndpointX.Models.DTO
{
    public class UpdatePermissionDto
    {
        [Required(ErrorMessage = "UserName is required")]
        public string UserName { get; set; }
    }
}
