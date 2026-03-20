using Plugin.Maui.Audio;

namespace MauiApp8.Services;

/// <summary>
/// Cross-platform audio recording and playback service using Plugin.Maui.Audio
/// </summary>
public class AudioService : IAudioService
{
    private readonly IAudioManager _audioManager;
    private IAudioRecorder? _audioRecorder;
    private IAudioPlayer? _audioPlayer;
    private string? _recordedFilePath;
    private readonly HttpClient _httpClient;

    public bool IsRecording => _audioRecorder?.IsRecording ?? false;
    public bool HasRecording => !string.IsNullOrEmpty(_recordedFilePath) && File.Exists(_recordedFilePath);
    public string RecordedFilePath => _recordedFilePath ?? string.Empty;
    public int LastRecordingSampleRate { get; private set; } = 44100;

    public event EventHandler<string>? RecordingStatusChanged;
    public event EventHandler<string>? PlaybackStatusChanged;
    public event EventHandler<string>? ErrorOccurred;

    public AudioService(IAudioManager audioManager)
    {
        _audioManager = audioManager;
        _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
    }

    public async Task<bool> CheckAndRequestMicrophonePermissionAsync()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.Microphone>();

        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<Permissions.Microphone>();
        }

        return status == PermissionStatus.Granted;
    }

    public async Task<bool> StartRecordingAsync()
    {
        try
        {
            var hasPermission = await CheckAndRequestMicrophonePermissionAsync();
            if (!hasPermission)
            {
                ErrorOccurred?.Invoke(this, "Microphone permission is required to record audio.");
                return false;
            }

            _audioRecorder = _audioManager.CreateRecorder();
            await _audioRecorder.StartAsync();

            RecordingStatusChanged?.Invoke(this, "Recording started");
            return true;
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(this, $"Failed to start recording: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> StopRecordingAsync()
    {
        try
        {
            if (_audioRecorder == null || !_audioRecorder.IsRecording)
            {
                ErrorOccurred?.Invoke(this, "No active recording to stop.");
                return false;
            }

            var audioSource = await _audioRecorder.StopAsync();

            var fileName = $"recording_{DateTime.Now:yyyyMMdd_HHmmss}.wav";
            var filePath = Path.Combine(FileSystem.CacheDirectory, fileName);

            using (var fileStream = File.Create(filePath))
            {
                var stream = audioSource.GetAudioStream();
                await stream.CopyToAsync(fileStream);
            }

            _recordedFilePath = filePath;

            RecordingStatusChanged?.Invoke(this, "Recording saved successfully");
            return true;
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(this, $"Failed to stop recording: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> PlayLastRecordingAsync()
    {
        try
        {
            if (!HasRecording)
            {
                ErrorOccurred?.Invoke(this, "No recording found to play.");
                return false;
            }

            StopPlayback();

            _audioPlayer = _audioManager.CreatePlayer(File.OpenRead(_recordedFilePath!));
            _audioPlayer.Play();

            PlaybackStatusChanged?.Invoke(this, "Playing");

            // Wait for playback to finish
            var duration = _audioPlayer.Duration;
            await Task.Delay((int)(duration * 1000) + 200);

            PlaybackStatusChanged?.Invoke(this, "Playback finished");
            return true;
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(this, $"Failed to play recording: {ex.Message}");
            PlaybackStatusChanged?.Invoke(this, "Playback error");
            return false;
        }
    }

    public async Task<bool> PlayFromUrlAsync(string url)
    {
        try
        {
            if (string.IsNullOrEmpty(url))
            {
                ErrorOccurred?.Invoke(this, "No audio URL provided.");
                return false;
            }

            StopPlayback();

            // Download the audio to a temp file
            var tempPath = Path.Combine(FileSystem.CacheDirectory, $"playback_{Guid.NewGuid()}.wav");
            var audioBytes = await _httpClient.GetByteArrayAsync(url);
            await File.WriteAllBytesAsync(tempPath, audioBytes);

            _audioPlayer = _audioManager.CreatePlayer(File.OpenRead(tempPath));
            _audioPlayer.Play();

            PlaybackStatusChanged?.Invoke(this, "Playing");

            var duration = _audioPlayer.Duration;
            await Task.Delay((int)(duration * 1000) + 200);

            PlaybackStatusChanged?.Invoke(this, "Playback finished");

            // Clean up temp file
            try { File.Delete(tempPath); } catch { }

            return true;
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(this, $"Failed to play audio: {ex.Message}");
            return false;
        }
    }

    public void StopPlayback()
    {
        if (_audioPlayer != null)
        {
            try
            {
                _audioPlayer.Stop();
                _audioPlayer.Dispose();
            }
            catch { }
            _audioPlayer = null;
            PlaybackStatusChanged?.Invoke(this, "Playback stopped");
        }
    }

    public bool DeleteLastRecording()
    {
        try
        {
            if (HasRecording)
            {
                File.Delete(_recordedFilePath!);
                _recordedFilePath = null;
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(this, $"Failed to delete recording: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Reads the last recorded WAV file, parses the header to get format info,
    /// converts stereo to mono if needed, and returns raw PCM 16-bit mono bytes for FFT analysis.
    /// </summary>
    public byte[]? GetLastRecordingPcmBytes()
    {
        try
        {
            if (!HasRecording)
                return null;

            var allBytes = File.ReadAllBytes(_recordedFilePath!);

            // Standard WAV header is at least 44 bytes
            if (allBytes.Length < 44)
                return null;

            // Check for "RIFF" magic bytes
            if (allBytes[0] != 'R' || allBytes[1] != 'I' || allBytes[2] != 'F' || allBytes[3] != 'F')
            {
                // Not a standard WAV, return all bytes and hope for the best
                return allBytes;
            }

            // Parse WAV format from "fmt " chunk
            int numChannels = 1;
            int bitsPerSample = 16;
            int wavSampleRate = 44100;
            int fmtOffset = 12;

            while (fmtOffset < allBytes.Length - 8)
            {
                string fmtChunkId = System.Text.Encoding.ASCII.GetString(allBytes, fmtOffset, 4);
                int fmtChunkSize = BitConverter.ToInt32(allBytes, fmtOffset + 4);

                if (fmtChunkId == "fmt ")
                {
                    numChannels = BitConverter.ToInt16(allBytes, fmtOffset + 10);
                    wavSampleRate = BitConverter.ToInt32(allBytes, fmtOffset + 12);
                    bitsPerSample = BitConverter.ToInt16(allBytes, fmtOffset + 22);
                    LastRecordingSampleRate = wavSampleRate;
                    Console.WriteLine($"WAV format: {numChannels} ch, {wavSampleRate} Hz, {bitsPerSample} bit");
                    break;
                }

                fmtOffset += 8 + fmtChunkSize;
                if (fmtChunkSize % 2 != 0) fmtOffset++;
            }

            // Find the "data" chunk
            int dataOffset = 12; // Skip past RIFF header
            while (dataOffset < allBytes.Length - 8)
            {
                string chunkId = System.Text.Encoding.ASCII.GetString(allBytes, dataOffset, 4);
                int chunkSize = BitConverter.ToInt32(allBytes, dataOffset + 4);

                if (chunkId == "data")
                {
                    int pcmStart = dataOffset + 8;
                    int pcmLength = Math.Min(chunkSize, allBytes.Length - pcmStart);
                    var rawPcm = new byte[pcmLength];
                    Array.Copy(allBytes, pcmStart, rawPcm, 0, pcmLength);

                    // Convert stereo to mono if needed
                    if (numChannels == 2 && bitsPerSample == 16)
                    {
                        return ConvertStereoToMono16(rawPcm);
                    }

                    return rawPcm;
                }

                dataOffset += 8 + chunkSize;
                if (chunkSize % 2 != 0) dataOffset++;
            }

            // Fallback: assume 44-byte header
            var fallbackPcm = new byte[allBytes.Length - 44];
            Array.Copy(allBytes, 44, fallbackPcm, 0, fallbackPcm.Length);
            return fallbackPcm;
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(this, $"Failed to read PCM data: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Convert stereo PCM 16-bit to mono by averaging left and right channels.
    /// </summary>
    private static byte[] ConvertStereoToMono16(byte[] stereoBytes)
    {
        int sampleCount = stereoBytes.Length / 4; // 2 bytes per sample * 2 channels
        var monoBytes = new byte[sampleCount * 2];

        for (int i = 0; i < sampleCount; i++)
        {
            int stereoOffset = i * 4;
            if (stereoOffset + 3 >= stereoBytes.Length) break;

            short left = BitConverter.ToInt16(stereoBytes, stereoOffset);
            short right = BitConverter.ToInt16(stereoBytes, stereoOffset + 2);
            short mono = (short)((left + right) / 2);

            var monoSampleBytes = BitConverter.GetBytes(mono);
            monoBytes[i * 2] = monoSampleBytes[0];
            monoBytes[i * 2 + 1] = monoSampleBytes[1];
        }

        Console.WriteLine($"Converted stereo ({stereoBytes.Length} bytes) to mono ({monoBytes.Length} bytes)");
        return monoBytes;
    }

    public void Dispose()
    {
        StopPlayback();
        _httpClient.Dispose();

        if (_audioRecorder != null && _audioRecorder.IsRecording)
        {
            try { _audioRecorder.StopAsync().Wait(); } catch { }
        }
    }
}
