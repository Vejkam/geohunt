using Moq;
using psi25_project.Models;
using psi25_project.Models.Dtos;
using psi25_project.Repositories.Interfaces;
using psi25_project.Services;
using psi25_project.Utils;

namespace Geohunt.Tests.Services
{
    public class LocationServiceTests
    {
        private readonly Mock<ILocationRepository> _mockRepo;
        private readonly ObjectValidator<LocationDto> _validator;
        private readonly LocationService _service;

        public LocationServiceTests()
        {
            _mockRepo = new Mock<ILocationRepository>();
            _validator = new ObjectValidator<LocationDto>();
            _service = new LocationService(_mockRepo.Object, _validator);
        }

        [Fact]
        public async Task GetAllLocationsAsync_ReturnsAllLocations()
        {
            // Arrange
            var locations = new List<Location>
            {
                new Location { Id = 1, Latitude = 10, Longitude = 20 },
                new Location { Id = 2, Latitude = 30, Longitude = 40 }
            };
            _mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(locations);

            // Act
            var result = await _service.GetAllLocationsAsync();

            // Assert
            Assert.Equal(2, result.Count());
            _mockRepo.Verify(r => r.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetRecentLocationsAsync_ReturnsRecentLocations()
        {
            // Arrange
            var recent = new List<object> { new { Id = 1 }, new { Id = 2 } };
            _mockRepo.Setup(r => r.GetRecentAsync(It.IsAny<DateTime>())).ReturnsAsync(recent);

            // Act
            var result = await _service.GetRecentLocationsAsync();

            // Assert
            Assert.Equal(2, result.Count());
            _mockRepo.Verify(r => r.GetRecentAsync(It.IsAny<DateTime>()), Times.Once);
        }

        [Fact]
        public async Task CreateLocationAsync_ValidDto_CreatesLocation()
        {
            // Arrange
            var dto = new LocationDto { Latitude = 10, Longitude = 20, panoId = "pano123" };
            _mockRepo.Setup(r => r.AddAsync(It.IsAny<Location>())).Returns(Task.CompletedTask);

            // Act
            var result = await _service.CreateLocationAsync(dto);

            // Assert
            Assert.Equal(dto.Latitude, result.Latitude);
            Assert.Equal(dto.Longitude, result.Longitude);
            Assert.Equal(dto.panoId, result.panoId);
            _mockRepo.Verify(r => r.AddAsync(It.IsAny<Location>()), Times.Once);
        }

        [Fact]
        public async Task CreateLocationAsync_NullPanoId_ThrowsArgumentNullException()
        {
            // Arrange
            var dto = new LocationDto { Latitude = 10, Longitude = 20, panoId = null! };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _service.CreateLocationAsync(dto));
        }

        [Fact]
        public async Task UpdateLastPlayedAsync_LocationExists_ReturnsSuccess()
        {
            // Arrange
            var loc = new Location { Id = 1, LastPlayedAt = DateTime.UtcNow.AddDays(-1) };
            _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(loc);
            _mockRepo.Setup(r => r.UpdateAsync(loc)).Returns(Task.CompletedTask);

            // Act
            var (success, result, message) = await _service.UpdateLastPlayedAsync(1);

            // Assert
            Assert.True(success);
            Assert.NotNull(result);
            Assert.Null(message);
            _mockRepo.Verify(r => r.GetByIdAsync(1), Times.Once);
            _mockRepo.Verify(r => r.UpdateAsync(loc), Times.Once);
        }

        [Fact]
        public async Task UpdateLastPlayedAsync_LocationNotFound_ReturnsFailure()
        {
            // Arrange
            _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Location?)null);

            // Act
            var (success, result, message) = await _service.UpdateLastPlayedAsync(1);

            // Assert
            Assert.False(success);
            Assert.Null(result);
            Assert.Equal("Location not found", message);
            _mockRepo.Verify(r => r.GetByIdAsync(1), Times.Once);
        }
    }
}
