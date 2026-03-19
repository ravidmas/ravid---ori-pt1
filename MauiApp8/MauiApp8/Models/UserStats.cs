using SQLite;

namespace MauiApp8.Models;

/// <summary>
/// Global user statistics for achievements and leaderboard scoring.
/// </summary>
[Table("UserStats")]
public class UserStats
{
    [PrimaryKey]
    public int Id { get; set; } = 1; // Singleton row

    public int TotalSessions { get; set; }
    public int TotalCorrect { get; set; }
    public int TotalAttempts { get; set; }
    public int CurrentStreak { get; set; }
    public int LongestStreak { get; set; }
    public int DaysActive { get; set; }
    public DateTime LastActiveDate { get; set; } = DateTime.MinValue;
    public int ConsecutiveCorrect { get; set; } // For "perfect session" tracking
    public int BestConsecutiveCorrect { get; set; }

    [Ignore]
    public double OverallAccuracy => TotalAttempts > 0
        ? (double)TotalCorrect / TotalAttempts * 100.0
        : 0;
}
