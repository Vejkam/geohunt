using System.ComponentModel.DataAnnotations;

namespace psi25_project.Models.Dtos
{
    public record LocationDto
    {
        [Required]
        public double Latitude { get; init; }

        [Required]
        public double Longitude { get; init; }

        [Required]
        public string panoId { get; init; } = string.Empty;
    }
}
