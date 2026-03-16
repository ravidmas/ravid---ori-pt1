using MauiApp8.Data;
using MauiApp8.Models;

namespace MauiApp8.Services;

/// <summary>
/// Manages achievements/trophies - checks conditions and unlocks new achievements after practice.
/// </summary>
public class AchievementService
{
    private readonly AppDatabase _database;

    // All defined achievements
    private static readonly List<AchievementDefinition> _definitions = new()
    {
        // ── Milestone Achievements ──
        new("first-note", "First Note", "Complete your first practice session",
            "\u2B50", "Milestone", 1, stats => stats.TotalSessions),

        new("getting-started", "Getting Started", "Master your first chord (80%+ accuracy, 5+ attempts)",
            "\uD83C\uDF31", "Milestone", 1, null), // Special check

        new("chord-collector", "Chord Collector", "Master 5 different chords",
            "\uD83C\uDFB8", "Milestone", 5, null), // Special check

        new("chord-master", "Chord Master", "Master 10 different chords",
            "\uD83D\uDC51", "Milestone", 10, null), // Special check

        new("dedicated-learner", "Dedicated Learner", "Practice for 7 days in a row",
            "\uD83D\uDD25", "Milestone", 7, stats => stats.CurrentStreak),

        new("unstoppable", "Unstoppable", "Practice for 30 days in a row",
            "\u26A1", "Milestone", 30, stats => stats.CurrentStreak),

        new("century", "Century", "Complete 100 practice sessions",
            "\uD83D\uDCAF", "Milestone", 100, stats => stats.TotalSessions),

        new("perfect-session", "Perfect Session", "Get 10 correct in a row",
            "\uD83C\uDFAF", "Milestone", 10, stats => stats.BestConsecutiveCorrect),

        // ── Skill Achievements ──
        new("eagle-ear", "Eagle Ear", "Detect a chord with 95%+ confidence",
            "\uD83D\uDC42", "Skill", 1, null), // Special check

        new("half-century", "Half Century", "Complete 50 practice sessions",
            "\uD83C\uDFC5", "Skill", 50, stats => stats.TotalSessions),

        new("sharp-shooter", "Sharp Shooter", "Achieve 80% overall accuracy",
            "\uD83C\uDFAF", "Skill", 80, null), // Special check on accuracy

        new("practice-warrior", "Practice Warrior", "Be active for 14 days total",
            "\u2694\uFE0F", "Skill", 14, stats => stats.DaysActive),
    };

    public AchievementService(AppDatabase database)
    {
        _database = database;
    }

    /// <summary>
    /// Initialize achievements in the database if they don't exist yet.
    /// </summary>
    public async Task InitializeAsync()
    {
        var existing = await _database.GetAllAchievementsAsync();
        foreach (var def in _definitions)
        {
            if (!existing.Any(a => a.Id == def.Id))
            {
                await _database.SaveAchievementAsync(new Achievement
                {
                    Id = def.Id,
                    Name = def.Name,
                    Description = def.Description,
                    IconEmoji = def.Icon,
                    Category = def.Category,
                    IsUnlocked = false,
                    ProgressCurrent = 0,
                    ProgressTarget = def.Target
                });
            }
        }
    }

    /// <summary>
    /// Check all achievements after a practice session. Returns newly unlocked achievements.
    /// </summary>
    public async Task<List<Achievement>> CheckAchievementsAsync(
        double lastConfidence = 0)
    {
        var stats = await _database.GetUserStatsAsync();
        var masteryList = await _database.GetAllChordMasteryAsync();
        var achievements = await _database.GetAllAchievementsAsync();
        var newlyUnlocked = new List<Achievement>();

        foreach (var achievement in achievements)
        {
            if (achievement.IsUnlocked) continue;

            var def = _definitions.FirstOrDefault(d => d.Id == achievement.Id);
            if (def == null) continue;

            int progress = 0;
            bool shouldUnlock = false;

            // Check based on definition
            if (def.StatsSelector != null)
            {
                progress = def.StatsSelector(stats);
                shouldUnlock = progress >= def.Target;
            }
            else
            {
                // Special checks
                switch (def.Id)
                {
                    case "getting-started":
                        var mastered = masteryList.Count(m => m.IsMastered);
                        progress = mastered;
                        shouldUnlock = mastered >= 1;
                        break;

                    case "chord-collector":
                        var masteredCount5 = masteryList.Count(m => m.IsMastered);
                        progress = masteredCount5;
                        shouldUnlock = masteredCount5 >= 5;
                        break;

                    case "chord-master":
                        var masteredCount10 = masteryList.Count(m => m.IsMastered);
                        progress = masteredCount10;
                        shouldUnlock = masteredCount10 >= 10;
                        break;

                    case "eagle-ear":
                        progress = lastConfidence >= 0.95 ? 1 : 0;
                        shouldUnlock = lastConfidence >= 0.95;
                        break;

                    case "sharp-shooter":
                        progress = (int)stats.OverallAccuracy;
                        shouldUnlock = stats.TotalAttempts >= 10 && stats.OverallAccuracy >= 80;
                        break;
                }
            }

            // Update progress
            achievement.ProgressCurrent = progress;
            await _database.SaveAchievementAsync(achievement);

            // Unlock if conditions met
            if (shouldUnlock)
            {
                achievement.IsUnlocked = true;
                achievement.UnlockedAt = DateTime.Now;
                await _database.SaveAchievementAsync(achievement);
                newlyUnlocked.Add(achievement);
            }
        }

        return newlyUnlocked;
    }

    private record AchievementDefinition(
        string Id, string Name, string Description,
        string Icon, string Category, int Target,
        Func<UserStats, int>? StatsSelector);
}
