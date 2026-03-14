namespace psi25_project
{
    public enum Direction
    {
        NORTH, SOUTH, EAST, WEST
    }
    public class DirectionPicker
    {
        public static (Direction direction1, Direction direction2) GetRandomDirections()
        {
            Array values = Enum.GetValues(typeof(Direction));

            Direction direction1 = (Direction)values.GetValue(Random.Shared.Next(values.Length))!;
            Direction direction2 = (Direction)values.GetValue(Random.Shared.Next(values.Length))!;

            return (direction1, direction2);
        }
    }

    public class CoordinateModifier
    {
        public static (double newLat, double newLng) ModifyCoordinates(double lat, double lng)
        {
            var (d1, d2) = DirectionPicker.GetRandomDirections();

            (lat, lng) = ApplyShift(lat, lng, d1);
            (lat, lng) = ApplyShift(lat, lng, d2);

            return (lat, lng);
        }
        private static (double, double) ApplyShift(double lat, double lng, Direction dir)
        {
            double meters = Random.Shared.NextDouble() * 20000;

            double metersPerDegreeLat = 111320.0;
            double metersPerDegreeLng = 111320.0 * Math.Cos(lat * Math.PI / 180.0);

            double deltaLat = meters / metersPerDegreeLat;
            double deltaLng = meters / metersPerDegreeLng;

            switch (dir)
            {
                case Direction.NORTH: lat += deltaLat; break;
                case Direction.SOUTH: lat -= deltaLat; break;
                case Direction.EAST: lng += deltaLng; break;
                case Direction.WEST: lng -= deltaLng; break;
            }

            return (lat, lng);
        }
    }
}
