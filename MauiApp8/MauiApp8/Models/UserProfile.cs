using SQLite;

namespace MauiApp8.Models;

/// <summary>
/// Local user profile for leaderboard identification and authentication.
/// </summary>
[Table("UserProfile")]
public class UserProfile
{
    [PrimaryKey]
    public string UserId { get; set; } = Guid.NewGuid().ToString();

    public string DisplayName { get; set; } = "Guitar Learner";
    public string AvatarEmoji { get; set; } = "\uD83C\uDFB8"; // Guitar emoji
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
