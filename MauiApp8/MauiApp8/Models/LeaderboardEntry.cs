namespace MauiApp8.Models;

/// <summary>
/// Represents a single entry in the leaderboard.
/// </summary>
public class LeaderboardEntry
{
    public int Rank { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string AvatarEmoji { get; set; } = "\uD83C\uDFB8";
    public int Score { get; set; }
    public int ChordsLearned { get; set; }
    public int TotalCorrect { get; set; }
    public bool IsCurrentUser { get; set; }
}
