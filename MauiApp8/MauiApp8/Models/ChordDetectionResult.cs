namespace MauiApp8.Models;

/// <summary>
/// Result of a chord detection analysis
/// </summary>
public class ChordDetectionResult
{
    /// <summary>
    /// The detected chord or note name (e.g. "A", "Em", "G")
    /// </summary>
    public string DetectedChordName { get; set; } = string.Empty;

    /// <summary>
    /// The dominant frequency detected in Hz
    /// </summary>
    public double DominantFrequencyHz { get; set; }

    /// <summary>
    /// Confidence of the detection (0.0 to 1.0)
    /// </summary>
    public double Confidence { get; set; }

    /// <summary>
    /// The nearest known note name (e.g. "A4", "E2")
    /// </summary>
    public string NearestNote { get; set; } = string.Empty;

    /// <summary>
    /// How many cents off from the nearest note (for tuning feedback)
    /// </summary>
    public double CentsOff { get; set; }

    /// <summary>
    /// Full FFT spectrum magnitudes (for visualization)
    /// </summary>
    public double[]? SpectrumMagnitudes { get; set; }

    /// <summary>
    /// Whether the detection is considered a match to a target chord
    /// </summary>
    public bool IsMatch(string targetChordName)
    {
        if (string.IsNullOrEmpty(targetChordName) || string.IsNullOrEmpty(DetectedChordName))
            return false;

        return DetectedChordName.Equals(targetChordName, StringComparison.OrdinalIgnoreCase);
    }
}
