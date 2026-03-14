using System.Threading.Tasks;
using psi25_project.Models.Dtos;

namespace psi25_project.Gateways.Interfaces
{
    public interface IGoogleMapsGateway
    {
        Task<GeocodeResultDto> GetCoordinatesAsync(string address);
        Task<StreetViewLocationDto?> GetStreetViewMetadataAsync(double lat, double lng);
    }
}
