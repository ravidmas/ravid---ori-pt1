using MauiApp8.Models;
using MauiApp8.Services;

namespace MauiApp8.Pages
{
    public partial class LearnChordsPage : ContentPage
    {
        private readonly IChordService _chordService;
        private readonly IAudioService _audioService;
        private string _lastDifficulty = "";

        public LearnChordsPage(IChordService chordService, IAudioService audioService)
        {
            InitializeComponent();
            _chordService = chordService;
            _audioService = audioService;
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void OnEasyTapped(object sender, EventArgs e)
        {
            await LoadChords("easy");
        }

        private async void OnMediumTapped(object sender, EventArgs e)
        {
            await LoadChords("medium");
        }

        private async void OnHardTapped(object sender, EventArgs e)
        {
            await LoadChords("hard");
        }

        private async Task LoadChords(string difficulty)
        {
            _lastDifficulty = difficulty;

            try
            {
                // Show skeleton loading placeholders
                SkeletonContainer.IsVisible = true;
                LoadingIndicator.IsVisible = true;
                LoadingIndicator.IsRunning = true;
                MessageContainer.IsVisible = false;
                RetryButton.IsVisible = false;

                // Clear previous chord cards (keep built-in UI elements)
                var toRemove = ChordsContainer.Children
                    .OfType<Frame>()
                    .ToList();
                foreach (var item in toRemove)
                    ChordsContainer.Children.Remove(item);

                // Fetch chords from server
                var chords = await _chordService.GetChordsByDifficultyAsync(difficulty);

                // Hide loading
                SkeletonContainer.IsVisible = false;
                LoadingIndicator.IsRunning = false;
                LoadingIndicator.IsVisible = false;

                if (chords == null || chords.Count == 0)
                {
                    NoChordsLabel.Text = $"No {difficulty} chords found";
                    MessageContainer.IsVisible = true;
                    RetryButton.IsVisible = true;
                    return;
                }

                MessageContainer.IsVisible = false;

                // Display chords
                foreach (var chord in chords)
                {
                    var chordFrame = CreateChordCard(chord);
                    ChordsContainer.Children.Add(chordFrame);
                }
            }
            catch (Exception)
            {
                SkeletonContainer.IsVisible = false;
                LoadingIndicator.IsRunning = false;
                LoadingIndicator.IsVisible = false;
                NoChordsLabel.Text = "Could not load chords. Please check your connection and try again.";
                MessageContainer.IsVisible = true;
                RetryButton.IsVisible = true;
            }
        }

        private async void OnRetryClicked(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(_lastDifficulty))
                await LoadChords(_lastDifficulty);
        }

        private Frame CreateChordCard(Chord chord)
        {
            var frame = new Frame
            {
                BackgroundColor = (Color)Application.Current!.Resources["AppCream"],
                CornerRadius = 15,
                Padding = new Thickness(15),
                HasShadow = true,
                Margin = new Thickness(0, 0, 0, 10)
            };

            var grid = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                    new ColumnDefinition { Width = GridLength.Auto }
                },
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto }
                }
            };

            // Chord Name
            var nameLabel = new Label
            {
                Text = chord.Name,
                FontSize = 24,
                FontAttributes = FontAttributes.Bold,
                TextColor = (Color)Application.Current!.Resources["AppDarkBrown"],
                VerticalOptions = LayoutOptions.Center
            };
            Grid.SetColumn(nameLabel, 0);
            Grid.SetRow(nameLabel, 0);

            // Difficulty Badge
            var difficultyLabel = new Label
            {
                Text = chord.Difficulty.ToUpper(),
                FontSize = 12,
                TextColor = Colors.White,
                BackgroundColor = GetDifficultyColor(chord.Difficulty),
                Padding = new Thickness(8, 4),
                HorizontalOptions = LayoutOptions.Start,
                Margin = new Thickness(0, 5, 0, 0)
            };
            Grid.SetColumn(difficultyLabel, 0);
            Grid.SetRow(difficultyLabel, 1);

            // Play Button
            var playButton = new Button
            {
                Text = "\u25B6 Play",
                BackgroundColor = (Color)Application.Current!.Resources["AppBrown"],
                TextColor = Colors.White,
                CornerRadius = 10,
                Padding = new Thickness(15, 10),
                VerticalOptions = LayoutOptions.Center
            };
            playButton.Clicked += (s, e) => OnPlayChord(chord);
            Grid.SetColumn(playButton, 1);
            Grid.SetRow(playButton, 0);
            Grid.SetRowSpan(playButton, 2);

            grid.Children.Add(nameLabel);
            grid.Children.Add(difficultyLabel);
            grid.Children.Add(playButton);

            frame.Content = grid;

            return frame;
        }

        private Color GetDifficultyColor(string difficulty)
        {
            var resources = Application.Current!.Resources;
            return difficulty.ToLower() switch
            {
                "easy" => (Color)resources["DifficultyEasy"],
                "medium" => (Color)resources["DifficultyMedium"],
                "hard" => (Color)resources["DifficultyHard"],
                _ => Color.FromArgb("#808080")
            };
        }

        private async void OnPlayChord(Chord chord)
        {
            if (string.IsNullOrEmpty(chord.SoundLink))
            {
                await DisplayAlert("No Sound", "This chord doesn't have a sound file yet", "OK");
                return;
            }

            try
            {
                await _audioService.PlayFromUrlAsync(chord.SoundLink);
            }
            catch (Exception)
            {
                await DisplayAlert("Playback Error", "Could not play the chord sound. Please try again.", "OK");
            }
        }
    }
}
