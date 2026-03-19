using MauiApp8.Data;
using MauiApp8.Models;

namespace MauiApp8.Pages;

public partial class TrophiesPage : ContentPage
{
    private readonly AppDatabase _database;
    private List<Achievement> _allAchievements = new();
    private string _currentFilter = "All";

    public TrophiesPage(AppDatabase database)
    {
        InitializeComponent();
        _database = database;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _allAchievements = await _database.GetAllAchievementsAsync();
        DisplayAchievements(_currentFilter);
    }

    private void DisplayAchievements(string filter)
    {
        _currentFilter = filter;
        TrophiesContainer.Children.Clear();

        var filtered = filter switch
        {
            "Milestone" => _allAchievements.Where(a => a.Category == "Milestone").ToList(),
            "Skill" => _allAchievements.Where(a => a.Category == "Skill").ToList(),
            _ => _allAchievements
        };

        // Unlocked first, then by progress
        var sorted = filtered
            .OrderByDescending(a => a.IsUnlocked)
            .ThenByDescending(a => a.ProgressPercent)
            .ToList();

        // Summary card
        var unlocked = _allAchievements.Count(a => a.IsUnlocked);
        var total = _allAchievements.Count;
        var summaryFrame = new Frame
        {
            BackgroundColor = (Color)Application.Current!.Resources["AppCream"],
            CornerRadius = 15,
            Padding = new Thickness(20),
            HasShadow = true
        };
        var summaryStack = new HorizontalStackLayout { Spacing = 10, HorizontalOptions = LayoutOptions.Center };
        summaryStack.Children.Add(new Label
        {
            Text = "\uD83C\uDFC6",
            FontSize = 36,
            VerticalOptions = LayoutOptions.Center
        });
        var summaryInfo = new VerticalStackLayout { Spacing = 2, VerticalOptions = LayoutOptions.Center };
        summaryInfo.Children.Add(new Label
        {
            Text = $"{unlocked} / {total} Unlocked",
            FontSize = 22,
            FontAttributes = FontAttributes.Bold,
            TextColor = (Color)Application.Current!.Resources["AppDarkBrown"]
        });
        summaryInfo.Children.Add(new Label
        {
            Text = unlocked == 0 ? "Start practicing to earn trophies!" : "Keep going!",
            FontSize = 13,
            TextColor = (Color)Application.Current!.Resources["Gray500"]
        });
        summaryStack.Children.Add(summaryInfo);
        summaryFrame.Content = summaryStack;
        TrophiesContainer.Children.Add(summaryFrame);

        // Achievement cards
        foreach (var achievement in sorted)
        {
            TrophiesContainer.Children.Add(CreateTrophyCard(achievement));
        }

        // Update tab styles
        UpdateTabStyles(filter);
    }

    private Frame CreateTrophyCard(Achievement achievement)
    {
        var frame = new Frame
        {
            BackgroundColor = achievement.IsUnlocked
                ? (Color)Application.Current!.Resources["AppCream"]
                : Color.FromArgb("#E0E0E0"),
            CornerRadius = 15,
            Padding = new Thickness(15),
            HasShadow = achievement.IsUnlocked,
            Opacity = achievement.IsUnlocked ? 1.0 : 0.7
        };

        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
            },
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto }
            },
            ColumnSpacing = 12,
            RowSpacing = 4
        };

        // Icon
        var iconLabel = new Label
        {
            Text = achievement.IsUnlocked ? achievement.IconEmoji : "\uD83D\uDD12",
            FontSize = 40,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Center
        };
        Grid.SetColumn(iconLabel, 0);
        Grid.SetRowSpan(iconLabel, 3);

        // Name
        var nameLabel = new Label
        {
            Text = achievement.Name,
            FontSize = 17,
            FontAttributes = FontAttributes.Bold,
            TextColor = achievement.IsUnlocked
                ? (Color)Application.Current!.Resources["AppDarkBrown"]
                : (Color)Application.Current!.Resources["Gray500"]
        };
        Grid.SetColumn(nameLabel, 1);
        Grid.SetRow(nameLabel, 0);

        // Description
        var descLabel = new Label
        {
            Text = achievement.Description,
            FontSize = 13,
            TextColor = (Color)Application.Current!.Resources["Gray500"]
        };
        Grid.SetColumn(descLabel, 1);
        Grid.SetRow(descLabel, 1);

        grid.Children.Add(iconLabel);
        grid.Children.Add(nameLabel);
        grid.Children.Add(descLabel);

        // Progress bar or unlock date
        if (achievement.IsUnlocked)
        {
            var unlockedLabel = new Label
            {
                Text = $"Unlocked {achievement.UnlockedAt?.ToString("MMM dd, yyyy") ?? ""}",
                FontSize = 11,
                TextColor = (Color)Application.Current!.Resources["SuccessGreen"]
            };
            Grid.SetColumn(unlockedLabel, 1);
            Grid.SetRow(unlockedLabel, 2);
            grid.Children.Add(unlockedLabel);
        }
        else if (achievement.ProgressTarget > 0)
        {
            // Progress bar
            var progressGrid = new Grid { ColumnDefinitions = { new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }, new ColumnDefinition { Width = GridLength.Auto } }, ColumnSpacing = 8 };

            var barBg = new Frame
            {
                BackgroundColor = (Color)Application.Current!.Resources["Gray200"],
                CornerRadius = 4,
                HeightRequest = 8,
                Padding = 0,
                HasShadow = false
            };
            var barFill = new Frame
            {
                BackgroundColor = (Color)Application.Current!.Resources["AppOrange"],
                CornerRadius = 4,
                HeightRequest = 8,
                Padding = 0,
                HasShadow = false,
                HorizontalOptions = LayoutOptions.Start,
                WidthRequest = Math.Max(0, achievement.ProgressPercent * 1.5)
            };
            barBg.Content = barFill;

            var progressText = new Label
            {
                Text = $"{achievement.ProgressCurrent}/{achievement.ProgressTarget}",
                FontSize = 11,
                TextColor = (Color)Application.Current!.Resources["Gray500"],
                VerticalOptions = LayoutOptions.Center
            };

            Grid.SetColumn(barBg, 0);
            Grid.SetColumn(progressText, 1);
            progressGrid.Children.Add(barBg);
            progressGrid.Children.Add(progressText);

            Grid.SetColumn(progressGrid, 1);
            Grid.SetRow(progressGrid, 2);
            grid.Children.Add(progressGrid);
        }

        frame.Content = grid;
        return frame;
    }

    private void UpdateTabStyles(string activeFilter)
    {
        AllTab.BackgroundColor = activeFilter == "All"
            ? (Color)Application.Current!.Resources["AppCream"]
            : (Color)Application.Current!.Resources["AppBrown"];
        AllTab.TextColor = activeFilter == "All"
            ? (Color)Application.Current!.Resources["AppDarkBrown"]
            : Colors.White;
        AllTab.Opacity = activeFilter == "All" ? 1 : 0.7;

        MilestoneTab.BackgroundColor = activeFilter == "Milestone"
            ? (Color)Application.Current!.Resources["AppCream"]
            : (Color)Application.Current!.Resources["AppBrown"];
        MilestoneTab.TextColor = activeFilter == "Milestone"
            ? (Color)Application.Current!.Resources["AppDarkBrown"]
            : Colors.White;
        MilestoneTab.Opacity = activeFilter == "Milestone" ? 1 : 0.7;

        SkillTab.BackgroundColor = activeFilter == "Skill"
            ? (Color)Application.Current!.Resources["AppCream"]
            : (Color)Application.Current!.Resources["AppBrown"];
        SkillTab.TextColor = activeFilter == "Skill"
            ? (Color)Application.Current!.Resources["AppDarkBrown"]
            : Colors.White;
        SkillTab.Opacity = activeFilter == "Skill" ? 1 : 0.7;
    }

    private void OnAllTabClicked(object sender, EventArgs e) => DisplayAchievements("All");
    private void OnMilestoneTabClicked(object sender, EventArgs e) => DisplayAchievements("Milestone");
    private void OnSkillTabClicked(object sender, EventArgs e) => DisplayAchievements("Skill");

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
