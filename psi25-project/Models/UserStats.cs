namespace psi25_project.Models;

public class UserStats
{
    public Guid UserId { get; set; }
    public int TotalGuesses { get; set; }
    public int TotalGames { get; set; }
    public int BestGameScore { get; set; }
    public int CurrentStreakDays { get; set; }
    public int LongestStreakDays { get; set; }
    public DateTime? LastPlayedDateUtc { get; set; }

    public ApplicationUser? User { get; set; }
}