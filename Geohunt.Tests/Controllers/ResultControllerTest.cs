using Microsoft.AspNetCore.Mvc;
using Moq;
using psi25_project.Controllers;
using psi25_project.Models.Dtos;
using psi25_project.Models;
using psi25_project.Services.Interfaces;

namespace Geohunt.Tests.Controllers
{
    public class ResultControllerTests
    {
        private readonly Mock<IResultService> _mockService;
        private readonly ResultController _controller;

        public ResultControllerTests()
        {
            _mockService = new Mock<IResultService>();
            _controller = new ResultController(_mockService.Object);
        }

        [Fact]
        public void GetResult_ReturnsOk_WithExpectedData()
        {
            // Arrange
            var dto = new DistanceDto
            {
                initialCoords = new Coordinate { Lat = 10, Lng = 20 },
                guessedCoords = new Coordinate { Lat = 11, Lng = 21 }
            };

            _mockService.Setup(s => s.CalculateResult(dto))
                        .Returns((5.5, 100));

            // Act
            IActionResult result = _controller.GetResult(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = okResult.Value;

            var initialCoords = (Coordinate)value.GetType().GetProperty("initialCoords")!.GetValue(value)!;
            var guessedCoords = (Coordinate)value.GetType().GetProperty("guessedCoords")!.GetValue(value)!;

            var distance = (double)value.GetType().GetProperty("distance")!.GetValue(value)!;
            var score = (int)value.GetType().GetProperty("score")!.GetValue(value)!;

            Assert.Equal(dto.initialCoords.Lat, initialCoords!.Lat);
            Assert.Equal(dto.initialCoords.Lng, initialCoords!.Lng);
            Assert.Equal(dto.guessedCoords.Lat, guessedCoords!.Lat);
            Assert.Equal(dto.guessedCoords.Lng, guessedCoords!.Lng);
            Assert.Equal(5.5, distance);
            Assert.Equal(100, score);
        }

        [Fact]
        public void GetResult_ReturnsBadRequest_WhenModelStateInvalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("initialCoords", "Required");

            var dto = new DistanceDto
            {
                initialCoords = new Coordinate { Lat = 0, Lng = 0 },
                guessedCoords = new Coordinate { Lat = 0, Lng = 0 }
            };

            // Act
            IActionResult result = _controller.GetResult(dto);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequest.Value);
        }

        [Fact]
        public void GetResult_ReturnsBadRequest_WhenServiceThrows()
        {
            // Arrange
            var dto = new DistanceDto
            {
                initialCoords = new Coordinate { Lat = 10, Lng = 20 },
                guessedCoords = new Coordinate { Lat = 11, Lng = 21 }
            };

            _mockService.Setup(s => s.CalculateResult(dto))
                        .Throws(new Exception("Something went wrong"));

            // Act
            IActionResult result = _controller.GetResult(dto);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var value = badRequest.Value;

            var error = value.GetType().GetProperty("error")!.GetValue(value) as string;

            Assert.Equal("Something went wrong", error);
        }
    }
}
