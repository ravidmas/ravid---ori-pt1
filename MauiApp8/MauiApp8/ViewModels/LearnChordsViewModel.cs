using System.Collections.ObjectModel;
using System.Windows.Input;
using MauiApp8.Models;
using MauiApp8.Services;

namespace MauiApp8.ViewModels;

public class LearnChordsViewModel : BaseViewModel
{
    private readonly IChordService _chordService;
    private readonly IAudioService _audioService;

    private string _selectedDifficulty = string.Empty;
    private bool _isLoading;
    private string _statusMessage = "Select a difficulty level to see chords";
    private bool _showStatus = true;

    public ObservableCollection<Chord> Chords { get; } = new();

    public string SelectedDifficulty
    {
        get => _selectedDifficulty;
        set => SetProperty(ref _selectedDifficulty, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public bool ShowStatus
    {
        get => _showStatus;
        set => SetProperty(ref _showStatus, value);
    }

    public ICommand LoadChordsCommand { get; }
    public ICommand PlayChordCommand { get; }

    public LearnChordsViewModel(IChordService chordService, IAudioService audioService)
    {
        _chordService = chordService;
        _audioService = audioService;

        LoadChordsCommand = new AsyncRelayCommand(async (param) =>
        {
            if (param is string difficulty)
                await LoadChords(difficulty);
        });

        PlayChordCommand = new AsyncRelayCommand(async (param) =>
        {
            if (param is Chord chord)
                await PlayChord(chord);
        });
    }

    private async Task LoadChords(string difficulty)
    {
        try
        {
            IsLoading = true;
            ShowStatus = false;
            SelectedDifficulty = difficulty;
            Chords.Clear();

            var chords = await _chordService.GetChordsByDifficultyAsync(difficulty);

            if (chords == null || chords.Count == 0)
            {
                StatusMessage = $"No {difficulty} chords found";
                ShowStatus = true;
                return;
            }

            foreach (var chord in chords)
            {
                Chords.Add(chord);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading chords: {ex.Message}");
            StatusMessage = "Could not load chords. Please check your connection and try again.";
            ShowStatus = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task PlayChord(Chord chord)
    {
        if (string.IsNullOrEmpty(chord.SoundLink))
            return;

        try
        {
            await _audioService.PlayFromUrlAsync(chord.SoundLink);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error playing chord: {ex.Message}");
        }
    }
}
