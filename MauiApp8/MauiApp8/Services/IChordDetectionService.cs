using MauiApp8.Models;

namespace MauiApp8.Services;

/// <summary>
/// Interface for detecting guitar chords from audio data
/// </summary>
public interface IChordDetectionService
{
    /// <summary>
    /// Detect the chord being played from raw PCM 16-bit mono audio data.
    /// </summary>
    /// <param name="pcm16MonoBytes">Raw PCM 16-bit mono audio bytes (no WAV header)</param>
    /// <param name="sampleRate">Sample rate of the audio (default 44100 Hz)</param>
    /// <returns>Detection result with chord name, frequency, and confidence</returns>
    ChordDetectionResult DetectChord(byte[] pcm16MonoBytes, int sampleRate = 44100);
}
