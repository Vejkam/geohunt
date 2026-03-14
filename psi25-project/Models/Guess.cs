namespace psi25_project.Models
{
    public class Guess
    {
        public int Id { get; set; }
        public Guid GameId { get; set; }
        public required Game Game { get; set; }
        public int LocationId { get; set; }
        public int RoundNumber { get; set; }
        public required Location Location { get; set; }
        public double GuessedLatitude { get; set; }
        public double GuessedLongitude { get; set; }
        public double DistanceKm { get; set; }
        public DateTime GuessedAt { get; set; }
        public int Score { get; set; }
    }
}
