using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;
using psi25_project.Services.Interfaces;

namespace Geohunt.Tests.Controllers
{
    public class GeocodingControllerTest
    {
        private readonly Mock<IGeocodingService> _mockService;
        private readonly GeocodingController _controller;

        public GeocodingControllerTest()
        {
            _mockService = new Mock<IGeocodingService>();
            _controller = new GeocodingController(_mockService.Object);
        }

        [Fact]
        public async Task GetValidCoordinates_ReturnsOk_WhenSuccessTrue()
        {
            // Arrange
            var expectedResult = new { lat = 54.123, lng = 23.456 };

            _mockService.Setup(s => s.GetValidCoordinatesAsync())
                        .ReturnsAsync((true, expectedResult));

            // Act
            var result = await _controller.GetValidCoordinates();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedResult, okResult.Value);
        }

        [Fact]
        public async Task GetValidCoordinates_ReturnsNotFound_WhenSuccessFalse()
        {
            // Arrange
            var errorMessage = "No valid coordinates found";

            _mockService.Setup(s => s.GetValidCoordinatesAsync())
                        .ReturnsAsync((false, errorMessage));

            // Act
            var result = await _controller.GetValidCoordinates();

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(errorMessage, notFound.Value);
        }

        [Fact]
        public async Task GetValidCoordinates_ReturnsBadRequest_OnException()
        {
            // Arrange
            _mockService.Setup(s => s.GetValidCoordinatesAsync())
                        .ThrowsAsync(new Exception("Something bad happened"));

            // Act
            var result = await _controller.GetValidCoordinates();

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);

            // Extract anonymous type property using reflection
            var errorProp = badRequest.Value.GetType().GetProperty("error");
            Assert.NotNull(errorProp);

            var errorValue = errorProp.GetValue(badRequest.Value) as string;
            Assert.Equal("Something bad happened", errorValue);
        }

    }
}
