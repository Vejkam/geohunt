using System;
using psi25_project;
using Xunit;

namespace Geohunt.Tests.Utils
{
    public class CoordinateModifierTests
    {
        [Fact]
        public void DirectionPicker_GetRandomDirections_ReturnsValidDirections()
        {
            for (int i = 0; i < 100; i++) // repeat to cover randomness
            {
                var (d1, d2) = DirectionPicker.GetRandomDirections();

                Assert.Contains(d1, Enum.GetValues<Direction>());
                Assert.Contains(d2, Enum.GetValues<Direction>());
            }
        }

        [Fact]
        public void CoordinateModifier_ModifyCoordinates_ReturnsCoordinatesWithinReasonableRange()
        {
            double lat = 50.0;
            double lng = 10.0;

            for (int i = 0; i < 100; i++)
            {
                var (newLat, newLng) = CoordinateModifier.ModifyCoordinates(lat, lng);

                Assert.InRange(newLat, lat - 1.0, lat + 1.0);

                Assert.InRange(newLng, lng - 1.0, lng + 1.0);
            }
        }

        [Fact]
        public void CoordinateModifier_ApplyShift_DoesNotReturnSameCoordinatesAlways()
        {
            double lat = 50.0;
            double lng = 10.0;

            bool changed = false;

            for (int i = 0; i < 50; i++)
            {
                var (newLat, newLng) = CoordinateModifier.ModifyCoordinates(lat, lng);

                if (newLat != lat || newLng != lng)
                {
                    changed = true;
                    break;
                }
            }

            Assert.True(changed, "Coordinates should change after applying shift.");
        }
    }
}
