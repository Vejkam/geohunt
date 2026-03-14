namespace psi25_project.Models.Dtos
{
    public class LeaderboardEntry : IComparable<LeaderboardEntry>
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public string? Username { get; set; }
        public double DistanceKm { get; set; }
        public DateTime GuessedAt { get; set; }
        public int TotalScore { get; set; }
        public int Rank { get; set; }

        public int CompareTo(LeaderboardEntry? other)
        {
            if (other == null) return -1;

            int scoreComparison = other.TotalScore.CompareTo(this.TotalScore);
            if (scoreComparison != 0) return scoreComparison;

            return this.DistanceKm.CompareTo(other.DistanceKm);
        }
    }
}
