namespace psi25_project.Services.Interfaces
{
    public interface IGeocodingService
    {
        Task<(bool success, object result)> GetValidCoordinatesAsync();
    }
}
