using System.Windows.Input;
using MauiApp8.Models;
using MauiApp8.Services;

namespace MauiApp8.ViewModels;

public class PracticeViewModel : BaseViewModel
{
    private readonly IAudioService _audioService;
    private readonly IChordDetectionService _chordDetectionService;
    private readonly IChordService _chordService;

    private Chord? _targetChord;
    private bool _isRecording;
    private bool _isAnalyzing;
    private bool _showResult;
    private string _statusText = "Tap the button below and play the chord";
    private string _timerText = string.Empty;
    private string _currentDifficulty = "easy";
    private ChordDetectionResult? _lastResult;
    private bool _isMatch;
    private CancellationTokenSource? _recordingCts;

    public Chord? TargetChord
    {
        get => _targetChord;
        set => SetProperty(ref _targetChord, value);
    }

    public bool IsRecording
    {
        get => _isRecording;
        set => SetProperty(ref _isRecording, value);
    }

    public bool IsAnalyzing
    {
        get => _isAnalyzing;
        set => SetProperty(ref _isAnalyzing, value);
    }

    public bool ShowResult
    {
        get => _showResult;
        set => SetProperty(ref _showResult, value);
    }

    public string StatusText
    {
        get => _statusText;
        set => SetProperty(ref _statusText, value);
    }

    public string TimerText
    {
        get => _timerText;
        set => SetProperty(ref _timerText, value);
    }

    public ChordDetectionResult? LastResult
    {
        get => _lastResult;
        set
        {
            SetProperty(ref _lastResult, value);
            OnPropertyChanged(nameof(DetectedChordText));
            OnPropertyChanged(nameof(FrequencyText));
            OnPropertyChanged(nameof(ConfidenceText));
            OnPropertyChanged(nameof(ResultTitle));
        }
    }

    public bool IsMatch
    {
        get => _isMatch;
        set => SetProperty(ref _isMatch, value);
    }

    public string DetectedChordText => LastResult != null ? $"Detected: {LastResult.DetectedChordName}" : "";
    public string FrequencyText => LastResult != null ? $"Frequency: {LastResult.DominantFrequencyHz:F1} Hz ({LastResult.NearestNote})" : "";
    public string ConfidenceText => LastResult != null ? $"Confidence: {LastResult.Confidence * 100:F0}%" : "";
    public string ResultTitle => IsMatch ? "\u2705 Correct!" : "\u274C Not quite";

    public ICommand RecordCommand { get; }
    public ICommand PlayReferenceCommand { get; }
    public ICommand NextChordCommand { get; }
    public ICommand TryAgainCommand { get; }

    public PracticeViewModel(
        IAudioService audioService,
        IChordDetectionService chordDetectionService,
        IChordService chordService)
    {
        _audioService = audioService;
        _chordDetectionService = chordDetectionService;
        _chordService = chordService;

        RecordCommand = new AsyncRelayCommand(OnRecord);
        PlayReferenceCommand = new AsyncRelayCommand(OnPlayReference);
        NextChordCommand = new AsyncRelayCommand(OnNextChord);
        TryAgainCommand = new RelayCommand(() =>
        {
            ShowResult = false;
            StatusText = "Tap the button below and play the chord";
        });
    }

    public async Task LoadRandomChordAsync()
    {
        try
        {
            StatusText = "Loading chord...";
            var chords = await _chordService.GetRandomChordsAsync(_currentDifficulty, 1);

            if (chords != null && chords.Count > 0)
            {
                TargetChord = chords[0];
                StatusText = "Tap the button below and play the chord";
                ShowResult = false;
            }
            else
            {
                chords = await _chordService.GetChordsByDifficultyAsync(_currentDifficulty);
                if (chords != null && chords.Count > 0)
                {
                    TargetChord = chords[new Random().Next(chords.Count)];
                    StatusText = "Tap the button below and play the chord";
                    ShowResult = false;
                }
                else
                {
                    StatusText = "Could not load chords. Check your connection.";
                }
            }
        }
        catch (Exception ex)
        {
            StatusText = "Failed to load chord. Check your connection.";
            Console.WriteLine($"LoadRandomChord error: {ex.Message}");
        }
    }

    private async Task OnRecord()
    {
        if (IsRecording)
            await StopAndAnalyze();
        else
            await StartRecording();
    }

    private async Task StartRecording()
    {
        if (TargetChord == null) return;

        IsRecording = true;
        ShowResult = false;
        StatusText = "Recording... Play the chord now!";

        var started = await _audioService.StartRecordingAsync();
        if (!started)
        {
            IsRecording = false;
            StatusText = "Failed to start recording. Check microphone permission.";
            return;
        }

        _recordingCts = new CancellationTokenSource();
        _ = RunCountdown(_recordingCts.Token);
    }

    private async Task RunCountdown(CancellationToken ct)
    {
        try
        {
            for (int i = 3; i > 0; i--)
            {
                if (ct.IsCancellationRequested) return;
                TimerText = $"{i}...";
                await Task.Delay(1000, ct);
            }

            if (!ct.IsCancellationRequested)
            {
                TimerText = "";
                await StopAndAnalyze();
            }
        }
        catch (TaskCanceledException) { }
    }

    private async Task StopAndAnalyze()
    {
        if (!IsRecording) return;

        _recordingCts?.Cancel();
        _recordingCts?.Dispose();
        _recordingCts = null;

        await _audioService.StopRecordingAsync();
        IsRecording = false;
        TimerText = "";

        IsAnalyzing = true;
        StatusText = "Analyzing...";

        await Task.Run(() =>
        {
            var pcmBytes = _audioService.GetLastRecordingPcmBytes();
            if (pcmBytes != null && pcmBytes.Length > 0)
            {
                var result = _chordDetectionService.DetectChord(pcmBytes);
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    LastResult = result;
                    IsMatch = result.IsMatch(TargetChord?.Name ?? "");
                    StatusText = IsMatch ? "Great job! You played it right!" : "Keep practicing, you'll get it!";
                    ShowResult = true;
                    IsAnalyzing = false;
                });
            }
            else
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    StatusText = "Could not read audio data. Try again.";
                    IsAnalyzing = false;
                });
            }
        });
    }

    private async Task OnPlayReference()
    {
        if (TargetChord == null || string.IsNullOrEmpty(TargetChord.SoundLink))
            return;

        try
        {
            await _audioService.PlayFromUrlAsync(TargetChord.SoundLink);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Play reference error: {ex.Message}");
        }
    }

    private async Task OnNextChord()
    {
        ShowResult = false;
        await LoadRandomChordAsync();
    }

    public void Cleanup()
    {
        _recordingCts?.Cancel();
        if (IsRecording)
        {
            _audioService.StopRecordingAsync();
            IsRecording = false;
        }
    }
}
