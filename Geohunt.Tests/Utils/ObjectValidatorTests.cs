using psi25_project.Models.Dtos;
using psi25_project.Utils;
using Xunit;

namespace Geohunt.Tests.Utils
{
    public class ObjectValidatorTests
    {
        private readonly ObjectValidator<LocationDto> _validator;

        public ObjectValidatorTests()
        {
            _validator = new ObjectValidator<LocationDto>();
        }

        [Fact]
        public void CreateDefault_ReturnsNewLocationDto()
        {
            // Act
            var result = _validator.CreateDefault();

            // Assert
            Assert.NotNull(result);
            Assert.IsType<LocationDto>(result);
        }

        [Fact]
        public void ValidatePropertyNotNull_PropertyIsNotNull_ReturnsTrue()
        {
            // Arrange
            var dto = new LocationDto
            {
                Latitude = 10.0,
                Longitude = 20.0,
                panoId = "abc123"
            };

            // Act
            var result = _validator.ValidatePropertyNotNull(dto, x => x.panoId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ValidatePropertyNotNull_PropertyIsNull_ReturnsFalse()
        {
            // Arrange
            var dto = new LocationDto
            {
                Latitude = 10.0,
                Longitude = 20.0,
                panoId = null
            };

            // Act
            var result = _validator.ValidatePropertyNotNull(dto, x => x.panoId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ValidatePropertyNotNull_AnotherPropertyNotNull_ReturnsTrue()
        {
            // Arrange
            var dto = new LocationDto
            {
                Latitude = 50.0,
                Longitude = 60.0,
                panoId = null
            };

            // Act
            var result = _validator.ValidatePropertyNotNull(dto, x => x.Latitude);

            // Assert
            Assert.True(result);
        }
    }
}
