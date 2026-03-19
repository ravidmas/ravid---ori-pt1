using MauiApp8.Data;
using MauiApp8.Models;
using MauiApp8.Services;

namespace MauiApp8.Pages;

public partial class PracticePage : ContentPage
{
    private readonly IAudioService _audioService;
    private readonly IChordDetectionService _chordDetectionService;
    private readonly IChordService _chordService;
    private readonly AppDatabase _database;
    private readonly AchievementService _achievementService;
    private readonly IAiService _aiService;

    private Chord? _targetChord;
    private bool _isRecording;
    private CancellationTokenSource? _recordingCts;
    private string _currentDifficulty = "easy";

    public PracticePage(
        IAudioService audioService,
        IChordDetectionService chordDetectionService,
        IChordService chordService,
        AppDatabase database,
        AchievementService achievementService,
        IAiService aiService)
    {
        InitializeComponent();
        _audioService = audioService;
        _chordDetectionService = chordDetectionService;
        _chordService = chordService;
        _database = database;
        _achievementService = achievementService;
        _aiService = aiService;

        // Subscribe to audio events
        _audioService.RecordingStatusChanged += OnRecordingStatusChanged;
        _audioService.ErrorOccurred += OnAudioError;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadRandomChord();
    }

    private async Task LoadRandomChord()
    {
        try
        {
            StatusLabel.Text = "Loading chord...";
            var chords = await _chordService.GetRandomChordsAsync(_currentDifficulty, 1);

            if (chords != null && chords.Count > 0)
            {
                _targetChord = chords[0];
                UpdateTargetChordDisplay();
                StatusLabel.Text = "Tap the button below and play the chord";
            }
            else
            {
                // Fallback: try getting all chords of this difficulty
                chords = await _chordService.GetChordsByDifficultyAsync(_currentDifficulty);
                if (chords != null && chords.Count > 0)
                {
                    var random = new Random();
                    _targetChord = chords[random.Next(chords.Count)];
                    UpdateTargetChordDisplay();
                    StatusLabel.Text = "Tap the button below and play the chord";
                }
                else
                {
                    TargetChordLabel.Text = "No chords";
                    StatusLabel.Text = "Could not load chords. Check your connection.";
                }
            }
        }
        catch (Exception ex)
        {
            StatusLabel.Text = "Failed to load chord. Check your connection.";
            Console.WriteLine($"LoadRandomChord error: {ex.Message}");
        }
    }

    private void UpdateTargetChordDisplay()
    {
        if (_targetChord == null) return;

        TargetChordLabel.Text = _targetChord.Name;

        // Set difficulty badge color
        var (color, text) = _targetChord.Difficulty?.ToLower() switch
        {
            "easy" => ((Color)Application.Current!.Resources["DifficultyEasy"], "Easy"),
            "medium" => ((Color)Application.Current!.Resources["DifficultyMedium"], "Medium"),
            "hard" => ((Color)Application.Current!.Resources["DifficultyHard"], "Hard"),
            _ => (Color.FromArgb("#808080"), _targetChord.Difficulty ?? "Unknown")
        };

        DifficultyBadge.BackgroundColor = color;
        DifficultyLabel.Text = text;

        // Show/hide play reference button based on whether sound link exists
        PlayReferenceButton.IsVisible = !string.IsNullOrEmpty(_targetChord.SoundLink);

        // Hide previous results
        ResultPanel.IsVisible = false;
    }

    private async void OnRecordTapped(object sender, EventArgs e)
    {
        if (_isRecording)
        {
            await StopRecordingAndAnalyze();
        }
        else
        {
            await StartRecording();
        }
    }

    private async Task StartRecording()
    {
        if (_targetChord == null)
        {
            await DisplayAlert("Error", "No target chord loaded. Please wait.", "OK");
            return;
        }

        _isRecording = true;
        ResultPanel.IsVisible = false;

        // Visual feedback - recording state
        RecordButtonFrame.BackgroundColor = Colors.Red;
        RecordButtonIcon.Text = "\u23F9"; // Stop icon
        StatusLabel.Text = "Recording... Play the chord now!";

        var started = await _audioService.StartRecordingAsync();
        if (!started)
        {
            _isRecording = false;
            RecordButtonFrame.BackgroundColor = (Color)Application.Current!.Resources["ErrorRed"];
            RecordButtonIcon.Text = "\uD83C\uDFA4"; // Mic icon
            StatusLabel.Text = "Failed to start recording. Check microphone permission.";
            return;
        }

        // Auto-stop after 3 seconds with countdown
        _recordingCts = new CancellationTokenSource();
        _ = RunRecordingCountdown(_recordingCts.Token);
    }

    private async Task RunRecordingCountdown(CancellationToken ct)
    {
        try
        {
            for (int i = 3; i > 0; i--)
            {
                if (ct.IsCancellationRequested) return;
                RecordTimerLabel.Text = $"{i}...";
                await Task.Delay(1000, ct);
            }

            if (!ct.IsCancellationRequested)
            {
                RecordTimerLabel.Text = "";
                await StopRecordingAndAnalyze();
            }
        }
        catch (TaskCanceledException) { }
    }

    private async Task StopRecordingAndAnalyze()
    {
        if (!_isRecording) return;

        _recordingCts?.Cancel();
        _recordingCts?.Dispose();
        _recordingCts = null;

        // Stop recording
        var stopped = await _audioService.StopRecordingAsync();
        _isRecording = false;

        // Reset visual state
        RecordButtonFrame.BackgroundColor = (Color)Application.Current!.Resources["ErrorRed"];
        RecordButtonIcon.Text = "\uD83C\uDFA4"; // Mic icon
        RecordTimerLabel.Text = "";

        if (!stopped)
        {
            StatusLabel.Text = "Recording failed. Tap to try again.";
            return;
        }

        // Analyze the recording
        StatusLabel.Text = "Analyzing...";
        LoadingIndicator.IsRunning = true;
        LoadingIndicator.IsVisible = true;

        await Task.Run(() =>
        {
            var pcmBytes = _audioService.GetLastRecordingPcmBytes();
            if (pcmBytes != null && pcmBytes.Length > 0)
            {
                var result = _chordDetectionService.DetectChord(pcmBytes);
                MainThread.BeginInvokeOnMainThread(() => ShowResult(result));
            }
            else
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    StatusLabel.Text = "Could not read audio data. Try again.";
                    LoadingIndicator.IsRunning = false;
                    LoadingIndicator.IsVisible = false;
                });
            }
        });
    }

    private async void ShowResult(ChordDetectionResult result)
    {
        LoadingIndicator.IsRunning = false;
        LoadingIndicator.IsVisible = false;

        bool isMatch = result.IsMatch(_targetChord?.Name ?? "");

        if (isMatch)
        {
            ResultTitleLabel.Text = "\u2705 Correct!";
            ResultTitleLabel.TextColor = (Color)Application.Current!.Resources["SuccessGreen"];
            StatusLabel.Text = "Great job! You played it right!";
        }
        else
        {
            ResultTitleLabel.Text = "\u274C Not quite";
            ResultTitleLabel.TextColor = (Color)Application.Current!.Resources["ErrorRed"];
            StatusLabel.Text = "Keep practicing, you'll get it!";
        }

        DetectedChordLabel.Text = $"Detected: {result.DetectedChordName}";
        FrequencyLabel.Text = $"Frequency: {result.DominantFrequencyHz:F1} Hz ({result.NearestNote})";
        ConfidenceLabel.Text = $"Confidence: {result.Confidence * 100:F0}%";

        ResultPanel.IsVisible = true;

        // Save practice result to SQLite
        try
        {
            var session = new PracticeSession
            {
                TargetChord = _targetChord?.Name ?? "",
                DetectedChord = result.DetectedChordName,
                IsCorrect = isMatch,
                Confidence = result.Confidence,
                FrequencyHz = result.DominantFrequencyHz,
                Timestamp = DateTime.Now,
                Difficulty = _currentDifficulty
            };

            await _database.SavePracticeSessionAsync(session);
            await _database.UpdateChordMasteryAsync(
                _targetChord?.Name ?? "", isMatch, _currentDifficulty);
            await _database.UpdateUserStatsAfterPracticeAsync(isMatch);

            // Update chords learned count in Preferences
            var masteredCount = await _database.GetMasteredChordCountAsync();
            Preferences.Set("ChordsLearned", masteredCount);

            // Check achievements
            var newTrophies = await _achievementService.CheckAchievementsAsync(result.Confidence);
            if (newTrophies.Count > 0)
            {
                foreach (var trophy in newTrophies)
                {
                    await DisplayAlert(
                        $"{trophy.IconEmoji} Trophy Unlocked!",
                        $"{trophy.Name}\n{trophy.Description}",
                        "Awesome!");
                }
            }

            // Get AI feedback (non-blocking, fire-and-forget)
            _ = GetAiFeedbackAsync(isMatch, result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to save practice result: {ex.Message}");
        }
    }

    private async Task GetAiFeedbackAsync(bool isMatch, ChordDetectionResult result)
    {
        if (!_aiService.IsConfigured) return;

        try
        {
            var prompt = $"The student just tried to play {_targetChord?.Name ?? "a chord"}. " +
                         $"Result: {(isMatch ? "correct" : $"incorrect, detected {result.DetectedChordName}")}. " +
                         $"Confidence: {result.Confidence * 100:F0}%. " +
                         "Give ONE encouraging sentence and ONE specific tip (max 30 words total).";

            var feedback = await _aiService.SendMessageAsync(prompt);

            MainThread.BeginInvokeOnMainThread(() =>
            {
                AiFeedbackLabel.Text = $"\uD83E\uDD16 {feedback}";
                AiFeedbackLabel.IsVisible = true;
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"AI feedback error: {ex.Message}");
        }
    }

    private async void OnPlayReferenceClicked(object sender, EventArgs e)
    {
        if (_targetChord == null || string.IsNullOrEmpty(_targetChord.SoundLink))
        {
            await DisplayAlert("No Sound", "This chord doesn't have a reference sound.", "OK");
            return;
        }

        PlayReferenceButton.Text = "Playing...";
        PlayReferenceButton.IsEnabled = false;

        try
        {
            await _audioService.PlayFromUrlAsync(_targetChord.SoundLink);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Play reference error: {ex.Message}");
        }
        finally
        {
            PlayReferenceButton.Text = "\u25B6 Play Reference Sound";
            PlayReferenceButton.IsEnabled = true;
        }
    }

    private async void OnTryAgainClicked(object sender, EventArgs e)
    {
        ResultPanel.IsVisible = false;
        StatusLabel.Text = "Tap the button below and play the chord";
    }

    private async void OnNextChordClicked(object sender, EventArgs e)
    {
        ResultPanel.IsVisible = false;
        await LoadRandomChord();
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private void OnRecordingStatusChanged(object? sender, string status)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            Console.WriteLine($"Recording status: {status}");
        });
    }

    private void OnAudioError(object? sender, string error)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await DisplayAlert("Audio Error", error, "OK");
        });
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _recordingCts?.Cancel();

        if (_isRecording)
        {
            _audioService.StopRecordingAsync();
            _isRecording = false;
        }
    }
}
