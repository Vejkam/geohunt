using System.ComponentModel.DataAnnotations;

namespace psi25_project.Models.Dtos
{
    public class TwoFactorDto
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Provider { get; set; } = "Email";

        [Required]
        public string Code { get; set; } = string.Empty;
    }
}
