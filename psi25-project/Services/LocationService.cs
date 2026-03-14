using psi25_project.Models;
using psi25_project.Models.Dtos;
using psi25_project.Repositories.Interfaces;
using psi25_project.Utils;

namespace psi25_project.Services
{
    public class LocationService : ILocationService
    {
        private readonly ILocationRepository _locationRepository;

        private readonly ObjectValidator<LocationDto> _validator;

        public LocationService(
            ILocationRepository locationRepository,
            ObjectValidator<LocationDto> validator)
        {
            _locationRepository = locationRepository;
            _validator = validator;
        }

        public async Task<IEnumerable<Location>> GetAllLocationsAsync()
        {
            return await _locationRepository.GetAllAsync();
        }

        public async Task<IEnumerable<object>> GetRecentLocationsAsync()
        {
            var cutoffUtc = DateTime.UtcNow.AddMonths(-6);
            return await _locationRepository.GetRecentAsync(cutoffUtc);
        }

        public async Task<Location> CreateLocationAsync(LocationDto dto)
        {
            if (!_validator.ValidatePropertyNotNull(dto, x => x.panoId))
                throw new ArgumentNullException(nameof(dto.panoId), "panoId cannot be null");

            var location = new Location
            {
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                panoId = dto.panoId,
                CreatedAt = DateTime.UtcNow,
                LastPlayedAt = DateTime.UtcNow,
                Guesses = new List<Guess>()
            };

            await _locationRepository.AddAsync(location);
            return location;
        }

        public async Task<(bool success, object result, string? message)> UpdateLastPlayedAsync(int id)
        {
            var location = await _locationRepository.GetByIdAsync(id);
            if (location == null)
                return (false, null!, "Location not found");

            location.LastPlayedAt = DateTime.UtcNow;
            await _locationRepository.UpdateAsync(location);

            return (true, new
            {
                message = "LastPlayedAt updated successfully",
                location.Id,
                location.LastPlayedAt
            }, null);
        }
    }
}
