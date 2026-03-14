using Microsoft.EntityFrameworkCore;
using psi25_project.Models;

namespace psi25_project.Data;

public class AchievementSeeder
{
    public static async Task SeedAchievements(GeoHuntContext context)
    {
        var achievements = new[]
        {
            new Achievement
            {
                Code = "FIRST_GUESS",
                Name = "First Guess",
                Description = "Make your first guess",
                Scope = AchievementScope.Meta,
                IsActive = true
            },
            new Achievement
            {
                Code = "BULLSEYE_100M",
                Name = "Bullseye",
                Description = "Guess within 100 m",
                Scope = AchievementScope.Round,
                IsActive = true
            },
            new Achievement
            {
                Code = "NEAR_1KM",
                Name = "Near Enough",
                Description = "Guess within 1 km",
                Scope = AchievementScope.Round,
                IsActive = true
            },
            new Achievement
            {
                Code = "SCORE_10K",
                Name = "Five Digits",
                Description = "Score 10,000+ points in a game",
                Scope = AchievementScope.Game,
                IsActive = true
            },
            new Achievement
            {
                Code = "CLEAN_SWEEP",
                Name = "Clean Sweep",
                Description = "All rounds in a game are <= 1 km distance",
                Scope = AchievementScope.Game,
                IsActive = true
            },
            new Achievement
            {
                Code = "STREAK_MASTER",
                Name = "Streak Master",
                Description = "Achieve a 30-day streak of playing every day",
                Scope = AchievementScope.Meta,
                IsActive = true
            },
            new Achievement
            {
                Code = "MARATHONER",
                Name = "The Marathoner",
                Description = "Play 100 games",
                Scope = AchievementScope.Meta,
                IsActive = true
            },
            new Achievement
            {
                Code = "LATE_NIGHT_PLAYER",
                Name = "Late Night Player",
                Description = "Play a game between midnight and 6 AM",
                Scope = AchievementScope.Game,
                IsActive = true
            }
        };

        var existingCodes = await context.Achievements
            .Select(a => a.Code)
            .ToListAsync();

        var newAchievements = achievements
            .Where(a => !existingCodes.Contains(a.Code))
            .ToList();

        if (newAchievements.Count > 0)
        {
            context.Achievements.AddRange(newAchievements);
            await context.SaveChangesAsync();
        }
    }
}
