using System.ComponentModel.DataAnnotations;

namespace psi25_project.Models.Dtos
{
    public class LoginUserDto
    {
        [Required]
        public string Username { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
    }
}