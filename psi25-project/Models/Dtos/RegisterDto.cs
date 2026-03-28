using System.ComponentModel.DataAnnotations;

namespace psi25_project.Models.Dtos
{
    public class RegisterDto
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        public bool EnableTwoFactor { get; set; }

        public string TwoFactorProvider { get; set; } = "Email";

        public string PhoneNumber { get; set; } = string.Empty;
    }
}
