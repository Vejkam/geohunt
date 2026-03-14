namespace psi25_project.Models;

public class UserAchievement
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public int AchievementId { get; set; }
    public DateTime UnlockedAt { get; set; }
    public ApplicationUser? User { get; set; }
    public Achievement? Achievement { get; set; }
}