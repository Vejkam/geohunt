namespace psi25_project.Models.Dtos
{
    public class CreateGuessDto
    {
        public Guid GameId { get; set; }
        public int LocationId { get; set; }
        public double GuessedLatitude { get; set; }
        public double GuessedLongitude { get; set; }
        public double DistanceKm { get; set; }
        public int Score { get; set; }
    }
}