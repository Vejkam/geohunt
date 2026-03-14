using System.ComponentModel.DataAnnotations;

namespace psi25_project.Models.Dtos
{
    public class UserResponseDto
    {
        [Required]
        public string Username { get; set; } = string.Empty;
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}