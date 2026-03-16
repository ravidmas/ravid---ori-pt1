namespace MauiApp8.Services;

/// <summary>
/// Interface for audio recording and playback operations
/// </summary>
public interface IAudioService : IDisposable
{
    bool IsRecording { get; }
    bool HasRecording { get; }
    string RecordedFilePath { get; }

    Task<bool> StartRecordingAsync();
    Task<bool> StopRecordingAsync();
    Task<bool> PlayLastRecordingAsync();
    Task<bool> PlayFromUrlAsync(string url);
    void StopPlayback();
    bool DeleteLastRecording();

    /// <summary>
    /// Returns raw PCM 16-bit mono bytes from the last recording (WAV header stripped).
    /// Used for FFT analysis and chord detection.
    /// </summary>
    byte[]? GetLastRecordingPcmBytes();

    event EventHandler<string>? RecordingStatusChanged;
    event EventHandler<string>? PlaybackStatusChanged;
    event EventHandler<string>? ErrorOccurred;
}
