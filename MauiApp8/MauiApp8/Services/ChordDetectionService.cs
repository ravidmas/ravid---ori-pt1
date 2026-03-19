using MauiApp8.Models;

namespace MauiApp8.Services;

/// <summary>
/// Detects guitar chords/notes from PCM audio data using FFT analysis.
/// Maps dominant frequency to the nearest musical note/chord.
/// </summary>
public class ChordDetectionService : IChordDetectionService
{
    // Standard guitar note frequencies (Hz) - covers open chords and common fretted notes
    // Organized by note name for chord root detection
    private static readonly (string Note, double Frequency)[] GuitarNotes = new[]
    {
        // 2nd octave (low E string open)
        ("E2", 82.41),
        ("F2", 87.31),
        ("F#2", 92.50),
        ("G2", 98.00),
        ("G#2", 103.83),
        ("A2", 110.00),
        ("A#2", 116.54),
        ("B2", 123.47),

        // 3rd octave
        ("C3", 130.81),
        ("C#3", 138.59),
        ("D3", 146.83),
        ("D#3", 155.56),
        ("E3", 164.81),
        ("F3", 174.61),
        ("F#3", 185.00),
        ("G3", 196.00),
        ("G#3", 207.65),
        ("A3", 220.00),
        ("A#3", 233.08),
        ("B3", 246.94),

        // 4th octave
        ("C4", 261.63),
        ("C#4", 277.18),
        ("D4", 293.66),
        ("D#4", 311.13),
        ("E4", 329.63),
        ("F4", 349.23),
        ("F#4", 369.99),
        ("G4", 392.00),
        ("G#4", 415.30),
        ("A4", 440.00),
        ("A#4", 466.16),
        ("B4", 493.88),

        // 5th octave (high E string upper frets)
        ("C5", 523.25),
        ("C#5", 554.37),
        ("D5", 587.33),
        ("D#5", 622.25),
        ("E5", 659.26),
        ("F5", 698.46),
        ("G5", 783.99),
        ("A5", 880.00),
        ("B5", 987.77),
    };

    // Common open guitar chord root notes
    // Maps a detected root note to the most likely chord name
    private static readonly Dictionary<string, string[]> NoteToChords = new()
    {
        ["E"] = new[] { "E", "Em" },
        ["A"] = new[] { "A", "Am" },
        ["D"] = new[] { "D", "Dm" },
        ["G"] = new[] { "G" },
        ["C"] = new[] { "C" },
        ["B"] = new[] { "B", "Bm", "B7" },
        ["F"] = new[] { "F", "Fm" },
    };

    public ChordDetectionResult DetectChord(byte[] pcm16MonoBytes, int sampleRate = 44100)
    {
        var result = new ChordDetectionResult();

        if (pcm16MonoBytes == null || pcm16MonoBytes.Length < 1024)
        {
            result.DetectedChordName = "Unknown";
            result.Confidence = 0;
            return result;
        }

        try
        {
            // Use FFT size of 4096 for good frequency resolution at guitar frequencies
            int fftSize = 4096;

            // Compute FFT
            var magnitudes = FftHelper.ComputeFftFromPcm16(pcm16MonoBytes, pcm16MonoBytes.Length, fftSize);

            // Smooth the spectrum to reduce noise
            magnitudes = FftHelper.SmoothMagnitudes(magnitudes, 3);

            // Get the dominant frequency
            double dominantFreq = FftHelper.GetDominantFrequency(magnitudes, sampleRate);

            // Filter out frequencies outside guitar range (roughly 75Hz - 1100Hz)
            if (dominantFreq < 75 || dominantFreq > 1100)
            {
                result.DetectedChordName = "Unknown";
                result.DominantFrequencyHz = dominantFreq;
                result.Confidence = 0;
                result.SpectrumMagnitudes = magnitudes;
                return result;
            }

            // Find the nearest note
            var (nearestNote, noteFreq, centsOff) = FindNearestNote(dominantFreq);

            // Calculate confidence based on peak prominence
            double confidence = CalculateConfidence(magnitudes, sampleRate, dominantFreq, fftSize);

            // Extract root note name (strip octave number)
            string rootNote = ExtractRootNote(nearestNote);

            // Map root note to chord name
            string chordName = MapNoteToChord(rootNote);

            result.DetectedChordName = chordName;
            result.DominantFrequencyHz = dominantFreq;
            result.Confidence = confidence;
            result.NearestNote = nearestNote;
            result.CentsOff = centsOff;
            result.SpectrumMagnitudes = magnitudes;

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Chord detection error: {ex.Message}");
            result.DetectedChordName = "Error";
            result.Confidence = 0;
            return result;
        }
    }

    /// <summary>
    /// Find the nearest musical note to a given frequency
    /// </summary>
    private (string Note, double Frequency, double CentsOff) FindNearestNote(double frequency)
    {
        string nearestNote = "Unknown";
        double nearestFreq = 0;
        double minDistance = double.MaxValue;

        foreach (var (note, noteFreq) in GuitarNotes)
        {
            // Use logarithmic distance (cents) for musical accuracy
            double cents = Math.Abs(1200 * Math.Log2(frequency / noteFreq));
            if (cents < minDistance)
            {
                minDistance = cents;
                nearestNote = note;
                nearestFreq = noteFreq;
            }
        }

        // Calculate signed cents off (positive = sharp, negative = flat)
        double signedCents = 1200 * Math.Log2(frequency / nearestFreq);

        return (nearestNote, nearestFreq, signedCents);
    }

    /// <summary>
    /// Calculate confidence based on how prominent the peak is relative to the average
    /// </summary>
    private double CalculateConfidence(double[] magnitudes, int sampleRate, double dominantFreq, int fftSize)
    {
        if (magnitudes.Length == 0) return 0;

        // Find the bin index of the dominant frequency
        double binWidth = (double)sampleRate / fftSize;
        int peakBin = (int)(dominantFreq / binWidth);

        if (peakBin >= magnitudes.Length) return 0;

        double peakMagnitude = magnitudes[peakBin];

        // Calculate average magnitude (excluding the peak region)
        double sum = 0;
        int count = 0;
        for (int i = 0; i < magnitudes.Length; i++)
        {
            if (Math.Abs(i - peakBin) > 10) // Exclude bins near the peak
            {
                sum += magnitudes[i];
                count++;
            }
        }

        double avgMagnitude = count > 0 ? sum / count : 0;

        if (avgMagnitude < 1e-10) return 0;

        // Signal-to-noise ratio as confidence
        double snr = peakMagnitude / avgMagnitude;

        // Normalize to 0-1 range (SNR of 10+ = high confidence)
        double confidence = Math.Min(1.0, snr / 15.0);

        return Math.Round(confidence, 2);
    }

    /// <summary>
    /// Extract the root note name without octave (e.g. "A4" -> "A", "C#3" -> "C#")
    /// </summary>
    private string ExtractRootNote(string noteWithOctave)
    {
        if (string.IsNullOrEmpty(noteWithOctave)) return "Unknown";

        // Remove the last character (octave number)
        int lastIndex = noteWithOctave.Length - 1;
        if (char.IsDigit(noteWithOctave[lastIndex]))
        {
            return noteWithOctave.Substring(0, lastIndex);
        }
        return noteWithOctave;
    }

    /// <summary>
    /// Map a root note to the most likely chord name.
    /// For v1, we return the root note as the chord name (e.g. "A" -> "A", "E" -> "E").
    /// Full chord quality detection (major vs minor) requires harmonic analysis in v2.
    /// </summary>
    private string MapNoteToChord(string rootNote)
    {
        // Strip sharp/flat for lookup
        string baseNote = rootNote.Replace("#", "").Replace("b", "");

        if (NoteToChords.ContainsKey(baseNote))
        {
            // Return the first (most common) chord for this root
            return NoteToChords[baseNote][0];
        }

        // For sharps/flats, return as-is
        return rootNote;
    }
}
