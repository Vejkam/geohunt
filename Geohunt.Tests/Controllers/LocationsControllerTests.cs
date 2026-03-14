using Microsoft.AspNetCore.Mvc;
using Moq;
using psi25_project.Models;
using psi25_project.Models.Dtos;
using psi25_project.Services;

namespace Geohunt.Tests.Controllers
{
    public class LocationsControllerTests
    {
        private readonly Mock<ILocationService> _mockService;
        private readonly LocationsController _controller;

        public LocationsControllerTests()
        {
            _mockService = new Mock<ILocationService>();
            _controller = new LocationsController(_mockService.Object);
        }

        [Fact]
        public async Task GetLocations_ReturnsOk_WithLocations()
        {
            // Arrange
            var locations = new List<Location>
            {
                new Location { Id = 1, Latitude = 10, Longitude = 20, panoId = "pano1" },
                new Location { Id = 2, Latitude = 30, Longitude = 40, panoId = "pano2" }
            };
            _mockService.Setup(s => s.GetAllLocationsAsync())
                        .ReturnsAsync(locations);

            // Act
            var result = await _controller.GetLocations();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var value = Assert.IsAssignableFrom<IEnumerable<Location>>(okResult.Value);
            Assert.Equal(2, ((List<Location>)value).Count);
        }

        [Fact]
        public async Task GetRecentLocations_ReturnsOk_WithRecentLocations()
        {
            // Arrange
            var recent = new List<object>
            {
                new { panoId = "pano1", Latitude = 10, Longitude = 20 }
            };
            _mockService.Setup(s => s.GetRecentLocationsAsync())
                        .ReturnsAsync(recent);

            // Act
            var result = await _controller.GetRecentLocations();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(recent, okResult.Value);
        }

        [Fact]
        public async Task CreateLocation_ReturnsCreated_WhenModelIsValid()
        {
            // Arrange
            var dto = new LocationDto { Latitude = 10, Longitude = 20, panoId = "pano123" };
            var created = new Location { Id = 1, Latitude = dto.Latitude, Longitude = dto.Longitude, panoId = dto.panoId };
            _mockService.Setup(s => s.CreateLocationAsync(dto))
                        .ReturnsAsync(created);

            // Act
            var result = await _controller.CreateLocation(dto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var value = Assert.IsType<Location>(createdResult.Value);
            Assert.Equal(dto.panoId, value.panoId);
        }

        [Fact]
        public async Task UpdateLastPlayed_ReturnsOk_WhenLocationExists()
        {
            // Arrange
            var response = (true, new { message = "Updated", Id = 1, LastPlayedAt = System.DateTime.UtcNow }, null as string);
            _mockService.Setup(s => s.UpdateLastPlayedAsync(1))
                        .ReturnsAsync(response);

            // Act
            var result = await _controller.UpdateLastPlayed(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = okResult.Value!;
            Assert.NotNull(value);
        }

        [Fact]
        public async Task UpdateLastPlayed_ReturnsNotFound_WhenLocationDoesNotExist()
        {
            // Arrange
            var response = (false, null as object, "Location not found");
            _mockService.Setup(s => s.UpdateLastPlayedAsync(99))
                        .ReturnsAsync(response);

            // Act
            var result = await _controller.UpdateLastPlayed(99);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Location not found", notFoundResult.Value);
        }
    }
}
