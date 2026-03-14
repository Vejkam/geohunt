namespace psi25_project.Models;

public class Achievement
{
    public int Id { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public AchievementScope Scope { get; set; }
    public bool IsActive { get; set; } = true;
}

public enum AchievementScope
{
    Round = 0,
    Game = 1,
    Meta = 2
}