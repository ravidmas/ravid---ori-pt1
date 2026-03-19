using System.Net.Http.Json;
using System.Text.Json;
using MauiApp8.Data;
using MauiApp8.Models;

namespace MauiApp8.Services;

/// <summary>
/// Manages leaderboard submission and retrieval.
/// Score formula: (ChordsLearned * 100) + (TotalCorrect * 10) + (LongestStreak * 50) + (AchievementsUnlocked * 200)
/// </summary>
public class LeaderboardService
{
    private readonly AppDatabase _database;
    private readonly HttpClient _httpClient;

    public LeaderboardService(AppDatabase database)
    {
        _database = database;
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(AppConfig.ApiBaseUrl),
            Timeout = TimeSpan.FromSeconds(AppConfig.HttpTimeoutSeconds)
        };
    }

    /// <summary>
    /// Calculate the user's score based on their stats and achievements.
    /// </summary>
    public async Task<int> CalculateScoreAsync()
    {
        var stats = await _database.GetUserStatsAsync();
        var masteredCount = await _database.GetMasteredChordCountAsync();
        var achievements = await _database.GetAllAchievementsAsync();
        var unlockedCount = achievements.Count(a => a.IsUnlocked);

        return (masteredCount * 100) +
               (stats.TotalCorrect * 10) +
               (stats.LongestStreak * 50) +
               (unlockedCount * 200);
    }

    /// <summary>
    /// Get or create the local user profile.
    /// </summary>
    public async Task<UserProfile> GetOrCreateProfileAsync()
    {
        var profile = await _database.GetUserProfileAsync();
        if (profile == null)
        {
            profile = new UserProfile();
            await _database.SaveUserProfileAsync(profile);
        }
        return profile;
    }

    /// <summary>
    /// Submit the user's score to the leaderboard API.
    /// Falls back gracefully if the server doesn't support leaderboard endpoints.
    /// </summary>
    public async Task<bool> SubmitScoreAsync()
    {
        try
        {
            var profile = await GetOrCreateProfileAsync();
            var score = await CalculateScoreAsync();
            var masteredCount = await _database.GetMasteredChordCountAsync();
            var stats = await _database.GetUserStatsAsync();

            var payload = new
            {
                userId = profile.UserId,
                displayName = profile.DisplayName,
                avatarEmoji = profile.AvatarEmoji,
                score,
                chordsLearned = masteredCount,
                totalCorrect = stats.TotalCorrect
            };

            var response = await _httpClient.PostAsJsonAsync("/leaderboard/submit", payload);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Leaderboard submit failed: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Get the leaderboard from the API. Returns local-only data if server is unavailable.
    /// </summary>
    public async Task<List<LeaderboardEntry>> GetLeaderboardAsync(string timeframe = "alltime")
    {
        try
        {
            var response = await _httpClient.GetAsync($"/leaderboard?timeframe={timeframe}");
            if (response.IsSuccessStatusCode)
            {
                var entries = await response.Content.ReadFromJsonAsync<List<LeaderboardEntry>>(
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (entries != null)
                {
                    // Mark current user
                    var profile = await GetOrCreateProfileAsync();
                    foreach (var entry in entries)
                    {
                        entry.IsCurrentUser = entry.UserId == profile.UserId;
                    }
                    return entries;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Leaderboard fetch failed: {ex.Message}");
        }

        // Fallback: return just the current user's data
        return await GetLocalLeaderboardAsync();
    }

    /// <summary>
    /// Generate a local leaderboard from all registered users.
    /// Uses the current user's real stats and generates simulated scores for other users
    /// based on their profile creation date (longer users = higher likely scores).
    /// </summary>
    private async Task<List<LeaderboardEntry>> GetLocalLeaderboardAsync()
    {
        var currentProfile = await GetOrCreateProfileAsync();
        var score = await CalculateScoreAsync();
        var masteredCount = await _database.GetMasteredChordCountAsync();
        var stats = await _database.GetUserStatsAsync();

        var entries = new List<LeaderboardEntry>();
        var allUsers = await _database.GetAllUsersAsync();

        foreach (var user in allUsers)
        {
            if (user.UserId == currentProfile.UserId)
            {
                entries.Add(new LeaderboardEntry
                {
                    UserId = user.UserId,
                    DisplayName = user.DisplayName,
                    AvatarEmoji = user.AvatarEmoji,
                    Score = score,
                    ChordsLearned = masteredCount,
                    TotalCorrect = stats.TotalCorrect,
                    IsCurrentUser = true
                });
            }
            else
            {
                // Other registered users show with zero score until they practice
                entries.Add(new LeaderboardEntry
                {
                    UserId = user.UserId,
                    DisplayName = user.DisplayName,
                    AvatarEmoji = user.AvatarEmoji,
                    Score = 0,
                    ChordsLearned = 0,
                    TotalCorrect = 0,
                    IsCurrentUser = false
                });
            }
        }

        // Sort by score descending and assign ranks
        entries = entries.OrderByDescending(e => e.Score).ToList();
        for (int i = 0; i < entries.Count; i++)
        {
            entries[i].Rank = i + 1;
        }

        return entries;
    }
}
