using System.ComponentModel.DataAnnotations;

namespace psi25_project.Models
{
    public enum GameState { Waiting, InProgress, Finished }

    public class MultiplayerGame
    {
        public Guid Id { get; set; }

        public Guid RoomId { get; set; }
        public Room Room { get; set; } = null!;
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public DateTime? FinishedAt { get; set; }
        public int CurrentRound { get; set; } = 1;
        public int TotalRounds { get; set; } = 1;
        public double? RoundLatitude { get; set; }
        public double? RoundLongitude { get; set; }
        public List<MultiplayerPlayer> Players { get; set; } = new();
        public GameState State { get; set; } = GameState.Waiting;
        [Timestamp]
        public byte[] RowVersion { get; set; } = null!;
    }
}
