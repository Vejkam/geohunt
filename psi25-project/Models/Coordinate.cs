namespace psi25_project.Models
{
    public readonly struct Coordinate
    {
        public double Lat { get; init; }
        public double Lng { get; init; }

        public Coordinate(double lat, double lng)
        {
            Lat = lat;
            Lng = lng;
        }
    }
}
