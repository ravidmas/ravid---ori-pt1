using SQLite;

namespace MauiApp8.Models;

/// <summary>
/// Represents a trophy/achievement that can be unlocked through practice.
/// </summary>
[Table("Achievements")]
public class Achievement
{
    [PrimaryKey]
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string IconEmoji { get; set; } = string.Empty;
    public string Category { get; set; } = "Milestone"; // "Milestone" or "Skill"
    public bool IsUnlocked { get; set; }
    public DateTime? UnlockedAt { get; set; }
    public int ProgressCurrent { get; set; }
    public int ProgressTarget { get; set; }

    [Ignore]
    public double ProgressPercent => ProgressTarget > 0
        ? Math.Min(100, (double)ProgressCurrent / ProgressTarget * 100)
        : 0;
}
