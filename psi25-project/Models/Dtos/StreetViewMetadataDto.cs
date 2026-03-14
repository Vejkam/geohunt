using System.Text.Json.Serialization;

namespace psi25_project.Models.Dtos
{
    public class StreetViewMetadataDto
    {
        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("pano_id")]
        public string? PanoId { get; set; }

        [JsonPropertyName("location")]
        public StreetViewLatLngDto? Location { get; set; }
    }

    public class StreetViewLatLngDto
    {
        [JsonPropertyName("lat")]
        public double Latitude { get; set; }

        [JsonPropertyName("lng")]
        public double Longitude { get; set; }
    }

    public class StreetViewLocationDto
    {
        public double Lat { get; set; }
        public double Lng { get; set; }
        public string? PanoId { get; set; }
    }
}
