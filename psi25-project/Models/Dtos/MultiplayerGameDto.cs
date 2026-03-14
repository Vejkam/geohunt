namespace psi25_project.Models.Dtos
{
    public class MultiplayerGameDto
    {
        public Guid GameId { get; set; }
        public Guid RoomId { get; set; }

        public int CurrentRound { get; set; }
        public int TotalRounds { get; set; }

        public double? RoundLatitude { get; set; }
        public double? RoundLongitude { get; set; }

        public List<MultiplayerPlayerDto> Players { get; set; } = new();
    }

    public class MultiplayerPlayerDto
    {
        public Guid PlayerId { get; set; }
        public string DisplayName { get; set; } = "";
        public int Score { get; set; }
        public bool Finished { get; set; }
        public double? LastGuessLatitude { get; set; }
        public double? LastGuessLongitude { get; set; }

        public double? DistanceMeters { get; set; }
    }

    public class RoundResultDto
    {
        public Guid PlayerId { get; set; }
        public int Score { get; set; }
        public double DistanceMeters { get; set; }
        public bool RoundFinished { get; set; }
    }

    public class GameResultDto
    {
        public Guid GameId { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? FinishedAt { get; set; }
        public List<PlayerScoreDto> PlayerScores { get; set; } = new();
    }
    public class PlayerScoreDto
    {
        public Guid PlayerId { get; set; }
        public string DisplayName { get; set; } = "";
        public int Score { get; set; }
    }
}
