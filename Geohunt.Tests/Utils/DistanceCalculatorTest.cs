using psi25_project.Models;
using psi25_project.Models.Dtos;
using psi25_project.Utils;
using Xunit;

namespace Geohunt.Tests.Utils
{
    public class DistanceCalculatorTests
    {
        [Fact]
        public void CalculateHaversineDistance_GeocodeResultDto_ReturnsExpectedDistance()
        {
            var point1 = new GeocodeResultDto { Lat = 40.7128, Lng = -74.0060 }; // NY
            var point2 = new GeocodeResultDto { Lat = 34.0522, Lng = -118.2437 }; // LA

            double distance = DistanceCalculator.CalculateHaversineDistance(point1, point2, precision: 1);

            Assert.InRange(distance, 3935.5, 3936.5);
        }

        [Fact]
        public void CalculateHaversineDistance_Coordinate_ReturnsExpectedDistance()
        {
            var coords1 = new Coordinate(51.5074, -0.1278); // London
            var coords2 = new Coordinate(48.8566, 2.3522);  // Paris

            double distance = DistanceCalculator.CalculateHaversineDistance(coords1, coords2);

            Assert.InRange(distance, 343.5, 344.0);
        }

        [Fact]
        public void CalculateHaversineDistance_SameCoordinates_ReturnsZero()
        {
            var coords = new Coordinate(10.0, 20.0);

            double distance = DistanceCalculator.CalculateHaversineDistance(coords, coords);

            Assert.Equal(0, distance);
        }
    }
}
