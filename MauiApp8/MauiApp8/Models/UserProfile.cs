using SQLite;

namespace MauiApp8.Models;

/// <summary>
/// Local user profile for leaderboard identification.
/// </summary>
[Table("UserProfile")]
public class UserProfile
{
    [PrimaryKey]
    public string UserId { get; set; } = Guid.NewGuid().ToString();

    public string DisplayName { get; set; } = "Guitar Learner";
    public string AvatarEmoji { get; set; } = "\uD83C\uDFB8"; // Guitar emoji
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
