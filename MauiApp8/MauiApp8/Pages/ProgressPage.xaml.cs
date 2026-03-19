using MauiApp8.Data;
using MauiApp8.Models;

namespace MauiApp8.Pages;

public partial class ProgressPage : ContentPage
{
    private readonly AppDatabase _database;

    public ProgressPage(AppDatabase database)
    {
        InitializeComponent();
        _database = database;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadProgressData();
    }

    private async Task LoadProgressData()
    {
        try
        {
            var stats = await _database.GetUserStatsAsync();
            var masteryList = await _database.GetAllChordMasteryAsync();
            var weakChords = await _database.GetWeakChordsAsync();
            var recentSessions = await _database.GetRecentSessionsAsync(10);

            // Overall stats
            TotalSessionsLabel.Text = stats.TotalSessions.ToString();
            ChordsLearnedLabel.Text = masteryList.Count(m => m.IsMastered).ToString();
            StreakLabel.Text = stats.CurrentStreak.ToString();

            var accuracy = stats.OverallAccuracy;
            AccuracyPercentLabel.Text = $"{accuracy:F0}%";
            AccuracyBar.WidthRequest = Math.Max(0, Math.Min(accuracy * 3, 300)); // Scale to fit

            // Chord mastery bars
            MasteryContainer.Children.Clear();
            if (masteryList.Count > 0)
            {
                NoMasteryLabel.IsVisible = false;
                foreach (var mastery in masteryList.OrderByDescending(m => m.MasteryPercent))
                {
                    MasteryContainer.Children.Add(CreateMasteryCard(mastery));
                }
            }
            else
            {
                NoMasteryLabel.IsVisible = true;
            }

            // Weak chords
            WeakChordsContainer.Children.Clear();
            if (weakChords.Count > 0)
            {
                WeakChordsHeader.IsVisible = true;
                foreach (var weak in weakChords)
                {
                    WeakChordsContainer.Children.Add(CreateWeakChordCard(weak));
                }
            }
            else
            {
                WeakChordsHeader.IsVisible = false;
            }

            // Recent history
            HistoryContainer.Children.Clear();
            if (recentSessions.Count > 0)
            {
                HistoryHeader.IsVisible = true;
                foreach (var session in recentSessions)
                {
                    HistoryContainer.Children.Add(CreateHistoryCard(session));
                }
            }
            else
            {
                HistoryHeader.IsVisible = false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading progress: {ex.Message}");
        }
    }

    private Frame CreateMasteryCard(ChordMastery mastery)
    {
        var frame = new Frame
        {
            BackgroundColor = (Color)Application.Current!.Resources["AppCream"],
            CornerRadius = 12,
            Padding = new Thickness(15, 10),
            HasShadow = true
        };

        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = GridLength.Auto }
            },
            ColumnSpacing = 10,
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto }
            },
            RowSpacing = 4
        };

        // Chord name
        var nameLabel = new Label
        {
            Text = mastery.ChordName,
            FontSize = 18,
            FontAttributes = FontAttributes.Bold,
            TextColor = (Color)Application.Current!.Resources["AppDarkBrown"]
        };
        Grid.SetColumn(nameLabel, 0);
        Grid.SetRow(nameLabel, 0);

        // Attempts info
        var attemptsLabel = new Label
        {
            Text = $"{mastery.CorrectAttempts}/{mastery.TotalAttempts} correct",
            FontSize = 12,
            TextColor = (Color)Application.Current!.Resources["Gray500"]
        };
        Grid.SetColumn(attemptsLabel, 0);
        Grid.SetRow(attemptsLabel, 1);

        // Mastery percent
        var percentColor = mastery.MasteryPercent >= 80
            ? (Color)Application.Current!.Resources["SuccessGreen"]
            : mastery.MasteryPercent >= 50
                ? (Color)Application.Current!.Resources["DifficultyMedium"]
                : (Color)Application.Current!.Resources["ErrorRed"];

        var percentLabel = new Label
        {
            Text = $"{mastery.MasteryPercent:F0}%",
            FontSize = 20,
            FontAttributes = FontAttributes.Bold,
            TextColor = percentColor,
            VerticalOptions = LayoutOptions.Center
        };
        Grid.SetColumn(percentLabel, 2);
        Grid.SetRowSpan(percentLabel, 2);

        // Progress bar
        var barBackground = new Frame
        {
            BackgroundColor = (Color)Application.Current!.Resources["Gray200"],
            CornerRadius = 3,
            HeightRequest = 6,
            Padding = 0,
            HasShadow = false
        };
        var barFill = new Frame
        {
            BackgroundColor = percentColor,
            CornerRadius = 3,
            HeightRequest = 6,
            Padding = 0,
            HasShadow = false,
            HorizontalOptions = LayoutOptions.Start,
            WidthRequest = Math.Max(0, mastery.MasteryPercent * 1.5) // Scale bar
        };
        barBackground.Content = barFill;
        Grid.SetColumn(barBackground, 1);
        Grid.SetRowSpan(barBackground, 2);

        grid.Children.Add(nameLabel);
        grid.Children.Add(attemptsLabel);
        grid.Children.Add(barBackground);
        grid.Children.Add(percentLabel);

        frame.Content = grid;
        return frame;
    }

    private Frame CreateWeakChordCard(ChordMastery mastery)
    {
        var frame = new Frame
        {
            BackgroundColor = Color.FromArgb("#FFF0F0"),
            CornerRadius = 12,
            Padding = new Thickness(15, 10),
            HasShadow = true
        };

        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
            },
            ColumnSpacing = 10
        };

        var icon = new Label
        {
            Text = "\u26A0\uFE0F",
            FontSize = 24,
            VerticalOptions = LayoutOptions.Center
        };

        var info = new VerticalStackLayout { Spacing = 2 };
        info.Children.Add(new Label
        {
            Text = mastery.ChordName,
            FontSize = 16,
            FontAttributes = FontAttributes.Bold,
            TextColor = (Color)Application.Current!.Resources["AppDarkBrown"]
        });
        info.Children.Add(new Label
        {
            Text = $"Only {mastery.MasteryPercent:F0}% accuracy - keep practicing!",
            FontSize = 12,
            TextColor = (Color)Application.Current!.Resources["ErrorRed"]
        });

        Grid.SetColumn(icon, 0);
        Grid.SetColumn(info, 1);
        grid.Children.Add(icon);
        grid.Children.Add(info);

        frame.Content = grid;
        return frame;
    }

    private Frame CreateHistoryCard(PracticeSession session)
    {
        var frame = new Frame
        {
            BackgroundColor = (Color)Application.Current!.Resources["AppCream"],
            CornerRadius = 10,
            Padding = new Thickness(12, 8),
            HasShadow = false
        };

        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = GridLength.Auto }
            },
            ColumnSpacing = 10
        };

        var icon = new Label
        {
            Text = session.IsCorrect ? "\u2705" : "\u274C",
            FontSize = 20,
            VerticalOptions = LayoutOptions.Center
        };

        var info = new VerticalStackLayout { Spacing = 1 };
        info.Children.Add(new Label
        {
            Text = $"Target: {session.TargetChord}",
            FontSize = 14,
            FontAttributes = FontAttributes.Bold,
            TextColor = (Color)Application.Current!.Resources["AppDarkBrown"]
        });
        info.Children.Add(new Label
        {
            Text = $"Detected: {session.DetectedChord}",
            FontSize = 12,
            TextColor = (Color)Application.Current!.Resources["Gray500"]
        });

        var timeLabel = new Label
        {
            Text = session.Timestamp.ToString("HH:mm"),
            FontSize = 12,
            TextColor = (Color)Application.Current!.Resources["Gray400"],
            VerticalOptions = LayoutOptions.Center
        };

        Grid.SetColumn(icon, 0);
        Grid.SetColumn(info, 1);
        Grid.SetColumn(timeLabel, 2);
        grid.Children.Add(icon);
        grid.Children.Add(info);
        grid.Children.Add(timeLabel);

        frame.Content = grid;
        return frame;
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
