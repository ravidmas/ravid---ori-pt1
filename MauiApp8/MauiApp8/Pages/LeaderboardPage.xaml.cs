using MauiApp8.Models;
using MauiApp8.Services;

namespace MauiApp8.Pages;

public partial class LeaderboardPage : ContentPage
{
    private readonly LeaderboardService _leaderboardService;
    private string _currentTimeframe = "alltime";

    public LeaderboardPage(LeaderboardService leaderboardService)
    {
        InitializeComponent();
        _leaderboardService = leaderboardService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadLeaderboard();
    }

    private async Task LoadLeaderboard()
    {
        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;
        EntriesContainer.Children.Clear();
        OfflineLabel.IsVisible = false;

        try
        {
            // Load user's own score
            var profile = await _leaderboardService.GetOrCreateProfileAsync();
            var score = await _leaderboardService.CalculateScoreAsync();
            YourAvatarLabel.Text = profile.AvatarEmoji;
            YourScoreLabel.Text = score.ToString("N0");

            // Submit score (background, don't block UI)
            _ = _leaderboardService.SubmitScoreAsync();

            // Fetch leaderboard
            var entries = await _leaderboardService.GetLeaderboardAsync(_currentTimeframe);

            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;

            // Find current user rank
            var userEntry = entries.FirstOrDefault(e => e.IsCurrentUser);
            YourRankLabel.Text = userEntry != null ? $"#{userEntry.Rank}" : "#-";

            // Check if we're showing local-only data
            if (entries.Count <= 1 && entries.All(e => e.IsCurrentUser))
            {
                OfflineLabel.IsVisible = true;
            }

            // Display entries
            foreach (var entry in entries)
            {
                EntriesContainer.Children.Add(CreateLeaderboardCard(entry));
            }
        }
        catch (Exception ex)
        {
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
            OfflineLabel.IsVisible = true;
            Console.WriteLine($"Leaderboard error: {ex.Message}");
        }
    }

    private Frame CreateLeaderboardCard(LeaderboardEntry entry)
    {
        var isTopThree = entry.Rank <= 3;
        var rankColor = entry.Rank switch
        {
            1 => Color.FromArgb("#FFD700"), // Gold
            2 => Color.FromArgb("#C0C0C0"), // Silver
            3 => Color.FromArgb("#CD7F32"), // Bronze
            _ => (Color)Application.Current!.Resources["Gray400"]
        };

        var frame = new Frame
        {
            BackgroundColor = entry.IsCurrentUser
                ? Color.FromArgb("#FFF5EB")
                : (Color)Application.Current!.Resources["AppCream"],
            CornerRadius = 12,
            Padding = new Thickness(12, 10),
            HasShadow = isTopThree,
            BorderColor = entry.IsCurrentUser
                ? (Color)Application.Current!.Resources["AppOrange"]
                : Colors.Transparent
        };

        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(40) },
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = GridLength.Auto }
            },
            ColumnSpacing = 10
        };

        // Rank
        var rankLabel = new Label
        {
            Text = isTopThree ? entry.Rank switch
            {
                1 => "\uD83E\uDD47",
                2 => "\uD83E\uDD48",
                3 => "\uD83E\uDD49",
                _ => $"#{entry.Rank}"
            } : $"#{entry.Rank}",
            FontSize = isTopThree ? 24 : 16,
            FontAttributes = FontAttributes.Bold,
            TextColor = rankColor,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };

        // Avatar
        var avatarLabel = new Label
        {
            Text = entry.AvatarEmoji,
            FontSize = 28,
            VerticalOptions = LayoutOptions.Center
        };

        // Name + details
        var nameStack = new VerticalStackLayout { Spacing = 1, VerticalOptions = LayoutOptions.Center };
        nameStack.Children.Add(new Label
        {
            Text = entry.DisplayName + (entry.IsCurrentUser ? " (You)" : ""),
            FontSize = 15,
            FontAttributes = entry.IsCurrentUser ? FontAttributes.Bold : FontAttributes.None,
            TextColor = (Color)Application.Current!.Resources["AppDarkBrown"]
        });
        nameStack.Children.Add(new Label
        {
            Text = $"{entry.ChordsLearned} chords mastered",
            FontSize = 11,
            TextColor = (Color)Application.Current!.Resources["Gray500"]
        });

        // Score
        var scoreLabel = new Label
        {
            Text = entry.Score.ToString("N0"),
            FontSize = 18,
            FontAttributes = FontAttributes.Bold,
            TextColor = (Color)Application.Current!.Resources["AppBrown"],
            VerticalOptions = LayoutOptions.Center
        };

        Grid.SetColumn(rankLabel, 0);
        Grid.SetColumn(avatarLabel, 1);
        Grid.SetColumn(nameStack, 2);
        Grid.SetColumn(scoreLabel, 3);

        grid.Children.Add(rankLabel);
        grid.Children.Add(avatarLabel);
        grid.Children.Add(nameStack);
        grid.Children.Add(scoreLabel);

        frame.Content = grid;
        return frame;
    }

    private void UpdateTabStyles(string active)
    {
        foreach (var (tab, name) in new[] { (WeekTab, "weekly"), (MonthTab, "monthly"), (AllTimeTab, "alltime") })
        {
            bool isActive = name == active;
            tab.BackgroundColor = isActive
                ? (Color)Application.Current!.Resources["AppCream"]
                : (Color)Application.Current!.Resources["AppBrown"];
            tab.TextColor = isActive
                ? (Color)Application.Current!.Resources["AppDarkBrown"]
                : Colors.White;
            tab.FontAttributes = isActive ? FontAttributes.Bold : FontAttributes.None;
            tab.Opacity = isActive ? 1 : 0.7;
        }
    }

    private async void OnWeekTabClicked(object sender, EventArgs e)
    {
        _currentTimeframe = "weekly";
        UpdateTabStyles("weekly");
        await LoadLeaderboard();
    }

    private async void OnMonthTabClicked(object sender, EventArgs e)
    {
        _currentTimeframe = "monthly";
        UpdateTabStyles("monthly");
        await LoadLeaderboard();
    }

    private async void OnAllTimeTabClicked(object sender, EventArgs e)
    {
        _currentTimeframe = "alltime";
        UpdateTabStyles("alltime");
        await LoadLeaderboard();
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
