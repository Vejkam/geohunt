using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using psi25_project.Models;

namespace psi25_project.Data
{
    public class GeoHuntContext 
        : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
    {
        public GeoHuntContext(DbContextOptions<GeoHuntContext> options)
            : base(options)
        {
        }

        public DbSet<Location> Locations { get; set; }
        public DbSet<Game> Games { get; set; }
        public DbSet<Guess> Guesses { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Player> Players { get; set; }
        public DbSet<MultiplayerGame> MultiplayerGames { get; set; }
        public DbSet<MultiplayerPlayer> MultiplayerPlayers { get; set; }
        public DbSet<Achievement> Achievements { get; set; } = default!;
        public DbSet<UserAchievement> UserAchievements { get; set; } = default!;
        public DbSet<UserStats> UserStats { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Game ↔ User
            modelBuilder.Entity<Game>()
                .HasOne(g => g.User)
                .WithMany(u => u.Games)
                .HasForeignKey(g => g.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Game ↔ Guess
            modelBuilder.Entity<Game>()
                .HasMany(g => g.Guesses)
                .WithOne(gu => gu.Game)
                .HasForeignKey(gu => gu.GameId)
                .OnDelete(DeleteBehavior.Cascade);

            // Guess ↔ Location
            modelBuilder.Entity<Guess>()
                .HasOne(gu => gu.Location)
                .WithMany(l => l.Guesses)
                .HasForeignKey(gu => gu.LocationId)
                .OnDelete(DeleteBehavior.Restrict);

            // Player ↔ Room
            modelBuilder.Entity<Player>()
                .HasOne(p => p.Room)
                .WithMany(r => r.Players)
                .HasForeignKey(p => p.RoomId)
                .OnDelete(DeleteBehavior.Cascade);

            //Player ↔ User
            modelBuilder.Entity<Player>()
                .HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MultiplayerGame>()
                .HasOne(g => g.Room)
                .WithMany()
                .HasForeignKey(g => g.RoomId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MultiplayerPlayer>()
                .HasOne(mp => mp.Game)
                .WithMany(g => g.Players)
                .HasForeignKey(mp => mp.GameId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MultiplayerPlayer>()
                .HasOne(mp => mp.Player)
                .WithMany()
                .HasForeignKey(mp => mp.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);

            // Unique coordinates
            modelBuilder.Entity<Location>()
                .HasIndex(l => new { l.Latitude, l.Longitude })
                .IsUnique();

            // Default timestamps
            modelBuilder.Entity<Location>()
                .Property(l => l.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            modelBuilder.Entity<Guess>()
                .Property(g => g.GuessedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            modelBuilder.Entity<Game>()
                .Property(g => g.StartedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            ConfigureAchievements(modelBuilder);
            ConfigureUserAchievements(modelBuilder);
            ConfigureUserStats(modelBuilder);
        }

        private static void ConfigureAchievements(ModelBuilder modelBuilder)
        {
            var entity = modelBuilder.Entity<Achievement>();

            entity.HasKey(a => a.Id);

            entity.Property(a => a.Code)
                .IsRequired()
                .HasMaxLength(64);

            entity.HasIndex(a => a.Code)
                .IsUnique();

            entity.Property(a => a.Name)
                .IsRequired()
                .HasMaxLength(128);

            entity.Property(a => a.Description)
                .IsRequired()
                .HasMaxLength(512);

            // Store enum as int in DB
            entity.Property(a => a.Scope)
                .HasConversion<int>();

            entity.Property(a => a.IsActive)
                .HasDefaultValue(true);
        }

        private static void ConfigureUserAchievements(ModelBuilder modelBuilder)
        {
            var entity = modelBuilder.Entity<UserAchievement>();

            entity.HasKey(ua => ua.Id);

            // Relationship: UserAchievement → Achievement (many-to-one)
            entity.HasOne(ua => ua.Achievement)
                .WithMany()
                .HasForeignKey(ua => ua.AchievementId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relationship: UserAchievement → ApplicationUser (many-to-one)
            entity.HasOne(ua => ua.User)
                .WithMany()
                .HasForeignKey(ua => ua.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(ua => new { ua.UserId, ua.AchievementId })
                .IsUnique();

            entity.Property(ua => ua.UnlockedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        }

        private static void ConfigureUserStats(ModelBuilder modelBuilder)
        {
            var entity = modelBuilder.Entity<UserStats>();

            entity.HasKey(us => us.UserId);

            entity.HasOne(us => us.User)
                .WithOne() 
                .HasForeignKey<UserStats>(us => us.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(us => us.TotalGuesses)
                .HasDefaultValue(0);

            entity.Property(us => us.TotalGames)
                .HasDefaultValue(0);

            entity.Property(us => us.BestGameScore)
                .HasDefaultValue(0);

            entity.Property(us => us.CurrentStreakDays)
                .HasDefaultValue(0);
        }
    }
}
