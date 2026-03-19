using SQLite;

namespace MauiApp8.Models;

/// <summary>
/// Tracks per-chord mastery level based on practice history.
/// </summary>
[Table("ChordMastery")]
public class ChordMastery
{
    [PrimaryKey]
    public string ChordName { get; set; } = string.Empty;

    public int TotalAttempts { get; set; }
    public int CorrectAttempts { get; set; }
    public DateTime LastPracticed { get; set; } = DateTime.MinValue;
    public string Difficulty { get; set; } = "easy";

    [Ignore]
    public double MasteryPercent => TotalAttempts > 0
        ? (double)CorrectAttempts / TotalAttempts * 100.0
        : 0;

    [Ignore]
    public bool IsMastered => MasteryPercent >= 80 && TotalAttempts >= 5;
}
