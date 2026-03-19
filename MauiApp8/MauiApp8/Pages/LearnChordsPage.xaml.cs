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

            var outerStack = new VerticalStackLayout { Spacing = 10 };

            // Top row: Name + Difficulty + Play button
            var topGrid = new Grid
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

            topGrid.Children.Add(nameLabel);
            topGrid.Children.Add(difficultyLabel);
            topGrid.Children.Add(playButton);

            outerStack.Children.Add(topGrid);

            // Chord Diagram
            if (chord.Frets != null && chord.Frets.Length == 6)
            {
                var diagram = CreateChordDiagram(chord);
                outerStack.Children.Add(diagram);
            }

            // Description
            if (!string.IsNullOrEmpty(chord.Description))
            {
                outerStack.Children.Add(new Label
                {
                    Text = chord.Description,
                    FontSize = 13,
                    TextColor = (Color)Application.Current!.Resources["Gray500"],
                    LineHeight = 1.3
                });
            }

            frame.Content = outerStack;
            return frame;
        }

        /// <summary>
        /// Creates a text-based chord diagram showing fret positions.
        /// </summary>
        private View CreateChordDiagram(Chord chord)
        {
            var diagramFrame = new Frame
            {
                BackgroundColor = Colors.White,
                CornerRadius = 10,
                Padding = new Thickness(15, 10),
                HasShadow = false
            };

            var stack = new VerticalStackLayout { Spacing = 2, HorizontalOptions = LayoutOptions.Center };

            string[] stringNames = { "E", "A", "D", "G", "B", "e" };

            // Header: string names
            var headerGrid = new Grid { ColumnSpacing = 0 };
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(28) });
            for (int f = 0; f <= 4; f++)
                headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(32) });

            // Each string row
            for (int s = 0; s < 6; s++)
            {
                var rowGrid = new Grid { ColumnSpacing = 0, HeightRequest = 22 };
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(28) });
                for (int f = 0; f <= 4; f++)
                    rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(32) });

                // String name
                var stringLabel = new Label
                {
                    Text = stringNames[s],
                    FontSize = 13,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = (Color)Application.Current!.Resources["AppDarkBrown"],
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center
                };
                Grid.SetColumn(stringLabel, 0);
                rowGrid.Children.Add(stringLabel);

                int fretValue = chord.Frets[s];
                int fingerValue = chord.Fingers != null && chord.Fingers.Length > s ? chord.Fingers[s] : 0;

                // Fret positions (columns 1-5 represent frets 1-5)
                for (int f = 1; f <= 4; f++)
                {
                    string cellText;
                    Color cellColor;

                    if (fretValue == -1 && f == 1)
                    {
                        cellText = "X";
                        cellColor = (Color)Application.Current!.Resources["ErrorRed"];
                    }
                    else if (fretValue == 0 && f == 1)
                    {
                        cellText = "O";
                        cellColor = (Color)Application.Current!.Resources["SuccessGreen"];
                    }
                    else if (fretValue == f)
                    {
                        cellText = fingerValue > 0 ? fingerValue.ToString() : "\u25CF";
                        cellColor = (Color)Application.Current!.Resources["AppBrown"];
                    }
                    else
                    {
                        cellText = "\u2500";
                        cellColor = Color.FromArgb("#CCCCCC");
                    }

                    var fretLabel = new Label
                    {
                        Text = cellText,
                        FontSize = fretValue == f ? 15 : 12,
                        FontAttributes = fretValue == f ? FontAttributes.Bold : FontAttributes.None,
                        TextColor = cellColor,
                        HorizontalTextAlignment = TextAlignment.Center,
                        VerticalTextAlignment = TextAlignment.Center
                    };
                    Grid.SetColumn(fretLabel, f);
                    rowGrid.Children.Add(fretLabel);
                }

                stack.Children.Add(rowGrid);
            }

            // Fret numbers footer
            var footerGrid = new Grid { ColumnSpacing = 0 };
            footerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(28) });
            for (int f = 1; f <= 4; f++)
            {
                footerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(32) });
                var fretNum = new Label
                {
                    Text = f.ToString(),
                    FontSize = 11,
                    TextColor = (Color)Application.Current!.Resources["Gray400"],
                    HorizontalTextAlignment = TextAlignment.Center
                };
                Grid.SetColumn(fretNum, f);
                footerGrid.Children.Add(fretNum);
            }
            stack.Children.Add(footerGrid);

            diagramFrame.Content = stack;
            return diagramFrame;
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
