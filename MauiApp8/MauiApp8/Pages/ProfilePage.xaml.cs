namespace MauiApp8.Pages;

public partial class ProfilePage : ContentPage
{
    public ProfilePage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadProfileData();
    }

    private void LoadProfileData()
    {
        // Load user name from preferences (will be enhanced with SQLite in Phase 5)
        var userName = Preferences.Get("UserDisplayName", "Guitar Learner");
        UserNameLabel.Text = userName;

        var joinDate = Preferences.Get("JoinDate", DateTime.Now.ToString("yyyy-MM-dd"));
        if (DateTime.TryParse(joinDate, out var date))
        {
            UserNameLabel.Text = userName;
            JoinDateLabel.Text = $"Learning since {date:MMMM yyyy}";
        }

        // Stats will be populated from SQLite in Phase 5
        TotalSessionsLabel.Text = Preferences.Get("TotalSessions", 0).ToString();
        var totalCorrect = Preferences.Get("TotalCorrect", 0);
        var totalAttempts = Preferences.Get("TotalAttempts", 0);
        AccuracyLabel.Text = totalAttempts > 0
            ? $"{(totalCorrect * 100.0 / totalAttempts):F0}%"
            : "0%";
        ChordsLearnedLabel.Text = Preferences.Get("ChordsLearned", 0).ToString();
        StreakLabel.Text = Preferences.Get("CurrentStreak", 0).ToString();

        // Set join date on first launch
        if (!Preferences.ContainsKey("JoinDate"))
        {
            Preferences.Set("JoinDate", DateTime.Now.ToString("yyyy-MM-dd"));
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnViewProgressTapped(object sender, EventArgs e)
    {
        var progressPage = Handler?.MauiContext?.Services.GetService<MauiApp8.Pages.ProgressPage>();
        if (progressPage != null)
            await Navigation.PushAsync(progressPage);
    }

    private async void OnViewTrophiesTapped(object sender, EventArgs e)
    {
        var trophiesPage = Handler?.MauiContext?.Services.GetService<MauiApp8.Pages.TrophiesPage>();
        if (trophiesPage != null)
            await Navigation.PushAsync(trophiesPage);
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Sign Out", "Are you sure you want to sign out?", "Yes", "Cancel");
        if (!confirm) return;

        Preferences.Remove("LoggedInUserId");
        Preferences.Remove("UserDisplayName");
        Preferences.Remove("UserEmail");

        if (Application.Current != null)
        {
            var loginPage = Handler?.MauiContext?.Services.GetService<MauiApp8.Pages.LoginPage>();
            if (loginPage != null)
                Application.Current.Windows[0].Page = new NavigationPage(loginPage);
        }
    }
}
