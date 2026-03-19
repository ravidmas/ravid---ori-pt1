using SQLite;
using MauiApp8.Models;

namespace MauiApp8.Data;

/// <summary>
/// Singleton SQLite database for storing practice sessions, chord mastery, and user stats.
/// </summary>
public class AppDatabase
{
    private SQLiteAsyncConnection? _database;
    private readonly string _dbPath;

    public AppDatabase()
    {
        _dbPath = Path.Combine(FileSystem.AppDataDirectory, "rdstrings.db3");
    }

    private async Task<SQLiteAsyncConnection> GetDatabaseAsync()
    {
        if (_database != null)
            return _database;

        _database = new SQLiteAsyncConnection(_dbPath);
        await _database.CreateTableAsync<PracticeSession>();
        await _database.CreateTableAsync<ChordMastery>();
        await _database.CreateTableAsync<UserStats>();
        await _database.CreateTableAsync<Achievement>();
        await _database.CreateTableAsync<UserProfile>();

        // Ensure a UserStats singleton row exists
        var stats = await _database.Table<UserStats>().FirstOrDefaultAsync();
        if (stats == null)
        {
            await _database.InsertAsync(new UserStats());
        }

        return _database;
    }

    // ── Practice Sessions ──

    public async Task SavePracticeSessionAsync(PracticeSession session)
    {
        var db = await GetDatabaseAsync();
        await db.InsertAsync(session);
    }

    public async Task<List<PracticeSession>> GetRecentSessionsAsync(int count = 50)
    {
        var db = await GetDatabaseAsync();
        return await db.Table<PracticeSession>()
            .OrderByDescending(s => s.Timestamp)
            .Take(count)
            .ToListAsync();
    }

    public async Task<List<PracticeSession>> GetAllSessionsAsync()
    {
        var db = await GetDatabaseAsync();
        return await db.Table<PracticeSession>().ToListAsync();
    }

    // ── Chord Mastery ──

    public async Task UpdateChordMasteryAsync(string chordName, bool wasCorrect, string difficulty)
    {
        var db = await GetDatabaseAsync();
        var mastery = await db.Table<ChordMastery>()
            .Where(m => m.ChordName == chordName)
            .FirstOrDefaultAsync();

        if (mastery == null)
        {
            mastery = new ChordMastery
            {
                ChordName = chordName,
                TotalAttempts = 1,
                CorrectAttempts = wasCorrect ? 1 : 0,
                LastPracticed = DateTime.Now,
                Difficulty = difficulty
            };
            await db.InsertAsync(mastery);
        }
        else
        {
            mastery.TotalAttempts++;
            if (wasCorrect) mastery.CorrectAttempts++;
            mastery.LastPracticed = DateTime.Now;
            await db.UpdateAsync(mastery);
        }
    }

    public async Task<List<ChordMastery>> GetAllChordMasteryAsync()
    {
        var db = await GetDatabaseAsync();
        return await db.Table<ChordMastery>()
            .OrderByDescending(m => m.LastPracticed)
            .ToListAsync();
    }

    public async Task<List<ChordMastery>> GetWeakChordsAsync(double threshold = 50)
    {
        var all = await GetAllChordMasteryAsync();
        return all.Where(m => m.MasteryPercent < threshold && m.TotalAttempts > 0).ToList();
    }

    public async Task<int> GetMasteredChordCountAsync()
    {
        var all = await GetAllChordMasteryAsync();
        return all.Count(m => m.IsMastered);
    }

    // ── User Stats ──

    public async Task<UserStats> GetUserStatsAsync()
    {
        var db = await GetDatabaseAsync();
        return await db.Table<UserStats>().FirstOrDefaultAsync() ?? new UserStats();
    }

    public async Task UpdateUserStatsAfterPracticeAsync(bool wasCorrect)
    {
        var db = await GetDatabaseAsync();
        var stats = await db.Table<UserStats>().FirstOrDefaultAsync() ?? new UserStats();

        stats.TotalSessions++;
        stats.TotalAttempts++;
        if (wasCorrect)
        {
            stats.TotalCorrect++;
            stats.ConsecutiveCorrect++;
            if (stats.ConsecutiveCorrect > stats.BestConsecutiveCorrect)
                stats.BestConsecutiveCorrect = stats.ConsecutiveCorrect;
        }
        else
        {
            stats.ConsecutiveCorrect = 0;
        }

        // Update streak
        var today = DateTime.Now.Date;
        if (stats.LastActiveDate.Date == today)
        {
            // Already active today, no streak change
        }
        else if (stats.LastActiveDate.Date == today.AddDays(-1))
        {
            // Consecutive day
            stats.CurrentStreak++;
            stats.DaysActive++;
            if (stats.CurrentStreak > stats.LongestStreak)
                stats.LongestStreak = stats.CurrentStreak;
        }
        else if (stats.LastActiveDate == DateTime.MinValue)
        {
            // First ever session
            stats.CurrentStreak = 1;
            stats.DaysActive = 1;
            stats.LongestStreak = 1;
        }
        else
        {
            // Streak broken
            stats.CurrentStreak = 1;
            stats.DaysActive++;
        }

        stats.LastActiveDate = today;
        await db.UpdateAsync(stats);

        // Also update Preferences for quick access on ProfilePage
        Preferences.Set("TotalSessions", stats.TotalSessions);
        Preferences.Set("TotalCorrect", stats.TotalCorrect);
        Preferences.Set("TotalAttempts", stats.TotalAttempts);
        Preferences.Set("CurrentStreak", stats.CurrentStreak);
    }

    // ── Achievements ──

    public async Task<List<Achievement>> GetAllAchievementsAsync()
    {
        var db = await GetDatabaseAsync();
        return await db.Table<Achievement>().ToListAsync();
    }

    public async Task SaveAchievementAsync(Achievement achievement)
    {
        var db = await GetDatabaseAsync();
        var existing = await db.Table<Achievement>()
            .Where(a => a.Id == achievement.Id)
            .FirstOrDefaultAsync();

        if (existing != null)
            await db.UpdateAsync(achievement);
        else
            await db.InsertAsync(achievement);
    }

    // ── User Profile ──

    public async Task<UserProfile?> GetUserProfileAsync()
    {
        var db = await GetDatabaseAsync();
        return await db.Table<UserProfile>().FirstOrDefaultAsync();
    }

    public async Task SaveUserProfileAsync(UserProfile profile)
    {
        var db = await GetDatabaseAsync();
        var existing = await db.Table<UserProfile>().FirstOrDefaultAsync();
        if (existing != null)
            await db.UpdateAsync(profile);
        else
            await db.InsertAsync(profile);
    }

    // ── Utility ──

    public async Task<int> GetTotalUniqueChordsPracticedAsync()
    {
        var db = await GetDatabaseAsync();
        var all = await db.Table<ChordMastery>().ToListAsync();
        return all.Count;
    }
}
