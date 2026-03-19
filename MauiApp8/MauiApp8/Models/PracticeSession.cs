using SQLite;

namespace MauiApp8.Models;

/// <summary>
/// Records a single practice attempt (one chord detection).
/// </summary>
[Table("PracticeSessions")]
public class PracticeSession
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string TargetChord { get; set; } = string.Empty;
    public string DetectedChord { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public double Confidence { get; set; }
    public double FrequencyHz { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public string Difficulty { get; set; } = "easy";
}
