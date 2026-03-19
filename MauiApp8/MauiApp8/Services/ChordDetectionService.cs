using MauiApp8.Models;

namespace MauiApp8.Services;

/// <summary>
/// Detects guitar chords from PCM audio data using FFT analysis with harmonic chord template matching.
/// Analyzes multiple peaks in the spectrum and matches against known chord frequency templates
/// to distinguish between major, minor, and 7th chords.
/// </summary>
public class ChordDetectionService : IChordDetectionService
{
    // Standard guitar note frequencies (Hz)
    private static readonly (string Note, double Frequency)[] GuitarNotes = new[]
    {
        ("E2", 82.41), ("F2", 87.31), ("F#2", 92.50), ("G2", 98.00), ("G#2", 103.83),
        ("A2", 110.00), ("A#2", 116.54), ("B2", 123.47),
        ("C3", 130.81), ("C#3", 138.59), ("D3", 146.83), ("D#3", 155.56),
        ("E3", 164.81), ("F3", 174.61), ("F#3", 185.00), ("G3", 196.00),
        ("G#3", 207.65), ("A3", 220.00), ("A#3", 233.08), ("B3", 246.94),
        ("C4", 261.63), ("C#4", 277.18), ("D4", 293.66), ("D#4", 311.13),
        ("E4", 329.63), ("F4", 349.23), ("F#4", 369.99), ("G4", 392.00),
        ("G#4", 415.30), ("A4", 440.00), ("A#4", 466.16), ("B4", 493.88),
        ("C5", 523.25), ("C#5", 554.37), ("D5", 587.33), ("D#5", 622.25),
        ("E5", 659.26), ("F5", 698.46), ("G5", 783.99), ("A5", 880.00), ("B5", 987.77),
    };

    // Chromatic note names for semitone-based analysis
    private static readonly string[] ChromaticNotes = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };

    /// <summary>
    /// Chord templates defined as intervals (semitones from root).
    /// Major: root, major third (4), perfect fifth (7)
    /// Minor: root, minor third (3), perfect fifth (7)
    /// 7th:   root, major third (4), perfect fifth (7), minor seventh (10)
    /// </summary>
    private static readonly Dictionary<string, int[]> ChordTemplates = new()
    {
        // Major chords
        ["E"] = new[] { 0, 4, 7 },
        ["A"] = new[] { 0, 4, 7 },
        ["D"] = new[] { 0, 4, 7 },
        ["G"] = new[] { 0, 4, 7 },
        ["C"] = new[] { 0, 4, 7 },
        ["F"] = new[] { 0, 4, 7 },
        ["B"] = new[] { 0, 4, 7 },
        ["Bb"] = new[] { 0, 4, 7 },

        // Minor chords
        ["Em"] = new[] { 0, 3, 7 },
        ["Am"] = new[] { 0, 3, 7 },
        ["Dm"] = new[] { 0, 3, 7 },
        ["Bm"] = new[] { 0, 3, 7 },
        ["F#m"] = new[] { 0, 3, 7 },
        ["C#m"] = new[] { 0, 3, 7 },
        ["G#m"] = new[] { 0, 3, 7 },
        ["Fm"] = new[] { 0, 3, 7 },

        // Dominant 7th chords
        ["E7"] = new[] { 0, 4, 7, 10 },
        ["A7"] = new[] { 0, 4, 7, 10 },
        ["D7"] = new[] { 0, 4, 7, 10 },
        ["B7"] = new[] { 0, 4, 7, 10 },
    };

    // Root note index in chromatic scale for each chord
    private static readonly Dictionary<string, int> ChordRoots = new()
    {
        ["C"] = 0, ["C#"] = 1, ["C#m"] = 1,
        ["D"] = 2, ["Dm"] = 2, ["D7"] = 2,
        ["D#"] = 3,
        ["E"] = 4, ["Em"] = 4, ["E7"] = 4,
        ["F"] = 5, ["Fm"] = 5, ["F#m"] = 6,
        ["G"] = 7, ["G#m"] = 8,
        ["A"] = 9, ["Am"] = 9, ["A7"] = 9,
        ["Bb"] = 10, ["B"] = 11, ["Bm"] = 11, ["B7"] = 11,
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
            int fftSize = 8192; // Larger FFT for better frequency resolution

            var magnitudes = FftHelper.ComputeFftFromPcm16(pcm16MonoBytes, pcm16MonoBytes.Length, fftSize);
            magnitudes = FftHelper.SmoothMagnitudes(magnitudes, 3);

            // Build a chroma vector (12-bin representation of pitch classes)
            double[] chroma = ComputeChromaVector(magnitudes, sampleRate, fftSize);

            // Find the dominant frequency for display
            double dominantFreq = GetDominantFrequencyInRange(magnitudes, sampleRate, fftSize, 75, 1100);

            if (dominantFreq < 75 || dominantFreq > 1100)
            {
                result.DetectedChordName = "Unknown";
                result.DominantFrequencyHz = dominantFreq;
                result.Confidence = 0;
                result.SpectrumMagnitudes = magnitudes;
                return result;
            }

            // Find nearest note for display
            var (nearestNote, noteFreq, centsOff) = FindNearestNote(dominantFreq);

            // Match chroma against chord templates
            var (chordName, confidence) = MatchChordTemplate(chroma, magnitudes, sampleRate, fftSize);

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
    /// Compute a 12-bin chroma vector from FFT magnitudes.
    /// Each bin represents one pitch class (C, C#, D, ..., B).
    /// Energy from all octaves is folded into the corresponding chroma bin.
    /// </summary>
    private double[] ComputeChromaVector(double[] magnitudes, int sampleRate, int fftSize)
    {
        double[] chroma = new double[12];
        double binWidth = (double)sampleRate / fftSize;

        // Only analyze bins in guitar frequency range (75-1100 Hz)
        int minBin = Math.Max(1, (int)(75.0 / binWidth));
        int maxBin = Math.Min(magnitudes.Length - 1, (int)(1100.0 / binWidth));

        for (int bin = minBin; bin <= maxBin; bin++)
        {
            double freq = bin * binWidth;
            if (freq < 60) continue;

            // Map frequency to nearest semitone
            // A4 = 440Hz, semitone = 12 * log2(freq / 440) + 69
            double semitone = 12.0 * Math.Log2(freq / 440.0) + 69;
            int chromaIndex = ((int)Math.Round(semitone) % 12 + 12) % 12;

            chroma[chromaIndex] += magnitudes[bin] * magnitudes[bin]; // Use energy (magnitude squared)
        }

        // Normalize
        double maxChroma = chroma.Max();
        if (maxChroma > 0)
        {
            for (int i = 0; i < 12; i++)
                chroma[i] /= maxChroma;
        }

        return chroma;
    }

    /// <summary>
    /// Match the computed chroma vector against all chord templates.
    /// Returns the best matching chord name and confidence score.
    /// </summary>
    private (string ChordName, double Confidence) MatchChordTemplate(double[] chroma, double[] magnitudes, int sampleRate, int fftSize)
    {
        string bestChord = "Unknown";
        double bestScore = -1;

        foreach (var (chordName, intervals) in ChordTemplates)
        {
            if (!ChordRoots.TryGetValue(chordName, out int rootIndex))
                continue;

            double score = 0;
            double totalWeight = 0;

            // Score: sum of chroma energy at expected intervals
            for (int i = 0; i < intervals.Length; i++)
            {
                int noteIndex = (rootIndex + intervals[i]) % 12;
                double weight = i == 0 ? 2.0 : 1.0; // Root note has double weight
                score += chroma[noteIndex] * weight;
                totalWeight += weight;
            }

            // Penalty: energy at non-chord tones
            double penalty = 0;
            for (int i = 0; i < 12; i++)
            {
                bool isChordTone = false;
                foreach (var interval in intervals)
                {
                    if ((rootIndex + interval) % 12 == i)
                    {
                        isChordTone = true;
                        break;
                    }
                }
                if (!isChordTone)
                {
                    penalty += chroma[i] * 0.3;
                }
            }

            double normalizedScore = (score / totalWeight) - (penalty / 12.0);

            if (normalizedScore > bestScore)
            {
                bestScore = normalizedScore;
                bestChord = chordName;
            }
        }

        // Convert score to 0-1 confidence
        double confidence = Math.Max(0, Math.Min(1.0, bestScore));

        // Boost confidence if signal is strong
        double signalStrength = CalculateSignalStrength(magnitudes, sampleRate, fftSize);
        confidence = confidence * 0.7 + signalStrength * 0.3;
        confidence = Math.Round(Math.Min(1.0, confidence), 2);

        return (bestChord, confidence);
    }

    /// <summary>
    /// Calculate overall signal strength as a confidence component.
    /// </summary>
    private double CalculateSignalStrength(double[] magnitudes, int sampleRate, int fftSize)
    {
        double binWidth = (double)sampleRate / fftSize;
        int minBin = Math.Max(1, (int)(75.0 / binWidth));
        int maxBin = Math.Min(magnitudes.Length - 1, (int)(1100.0 / binWidth));

        // Find peak magnitude in guitar range
        double peakMag = 0;
        for (int i = minBin; i <= maxBin; i++)
        {
            if (magnitudes[i] > peakMag)
                peakMag = magnitudes[i];
        }

        // Calculate average outside peak region
        double sum = 0;
        int count = 0;
        int peakBin = Array.IndexOf(magnitudes, peakMag);
        for (int i = minBin; i <= maxBin; i++)
        {
            if (Math.Abs(i - peakBin) > 20)
            {
                sum += magnitudes[i];
                count++;
            }
        }

        double avg = count > 0 ? sum / count : 0;
        if (avg < 1e-10) return 0;

        double snr = peakMag / avg;
        return Math.Min(1.0, snr / 20.0);
    }

    /// <summary>
    /// Get the dominant frequency within a specific Hz range.
    /// </summary>
    private double GetDominantFrequencyInRange(double[] magnitudes, int sampleRate, int fftSize, double minHz, double maxHz)
    {
        double binWidth = (double)sampleRate / fftSize;
        int minBin = Math.Max(1, (int)(minHz / binWidth));
        int maxBin = Math.Min(magnitudes.Length - 1, (int)(maxHz / binWidth));

        int peakBin = minBin;
        double peakMag = magnitudes[minBin];

        for (int i = minBin + 1; i <= maxBin; i++)
        {
            if (magnitudes[i] > peakMag)
            {
                peakMag = magnitudes[i];
                peakBin = i;
            }
        }

        // Parabolic interpolation for more accurate frequency estimation
        if (peakBin > 0 && peakBin < magnitudes.Length - 1)
        {
            double alpha = magnitudes[peakBin - 1];
            double beta = magnitudes[peakBin];
            double gamma = magnitudes[peakBin + 1];
            double denom = alpha - 2.0 * beta + gamma;
            if (Math.Abs(denom) > 1e-10)
            {
                double p = 0.5 * (alpha - gamma) / denom;
                return (peakBin + p) * binWidth;
            }
        }

        return peakBin * binWidth;
    }

    private (string Note, double Frequency, double CentsOff) FindNearestNote(double frequency)
    {
        string nearestNote = "Unknown";
        double nearestFreq = 0;
        double minDistance = double.MaxValue;

        foreach (var (note, noteFreq) in GuitarNotes)
        {
            double cents = Math.Abs(1200 * Math.Log2(frequency / noteFreq));
            if (cents < minDistance)
            {
                minDistance = cents;
                nearestNote = note;
                nearestFreq = noteFreq;
            }
        }

        double signedCents = 1200 * Math.Log2(frequency / nearestFreq);
        return (nearestNote, nearestFreq, signedCents);
    }
}
