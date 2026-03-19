namespace MauiApp8
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void OnLearnChordsTapped(object sender, EventArgs e)
        {
            var learnPage = Handler?.MauiContext?.Services.GetService<MauiApp8.Pages.LearnChordsPage>();
            if (learnPage != null)
                await Navigation.PushAsync(learnPage);
        }

        private async void OnPracticeTapped(object sender, EventArgs e)
        {
            var practicePage = Handler?.MauiContext?.Services.GetService<MauiApp8.Pages.PracticePage>();
            if (practicePage != null)
                await Navigation.PushAsync(practicePage);
        }

        private async void OnLessonsTapped(object sender, EventArgs e)
        {
            var lessonsPage = Handler?.MauiContext?.Services.GetService<MauiApp8.Pages.LessonsPage>();
            if (lessonsPage != null)
                await Navigation.PushAsync(lessonsPage);
        }

        private async void OnProgressTapped(object sender, EventArgs e)
        {
            var profilePage = Handler?.MauiContext?.Services.GetService<MauiApp8.Pages.ProfilePage>();
            if (profilePage != null)
                await Navigation.PushAsync(profilePage);
        }

        private void OnHomeTapped(object sender, EventArgs e)
        {
            // Already on home - no action needed
        }

        private async void OnLearnTapped(object sender, EventArgs e)
        {
            var learnPage = Handler?.MauiContext?.Services.GetService<MauiApp8.Pages.LearnChordsPage>();
            if (learnPage != null)
                await Navigation.PushAsync(learnPage);
        }

        private async void OnPracticeNavTapped(object sender, EventArgs e)
        {
            var practicePage = Handler?.MauiContext?.Services.GetService<MauiApp8.Pages.PracticePage>();
            if (practicePage != null)
                await Navigation.PushAsync(practicePage);
        }

        private async void OnProfileTapped(object sender, EventArgs e)
        {
            var profilePage = Handler?.MauiContext?.Services.GetService<MauiApp8.Pages.ProfilePage>();
            if (profilePage != null)
                await Navigation.PushAsync(profilePage);
        }

        private async void OnLeaderboardTapped(object sender, EventArgs e)
        {
            var leaderboardPage = Handler?.MauiContext?.Services.GetService<MauiApp8.Pages.LeaderboardPage>();
            if (leaderboardPage != null)
                await Navigation.PushAsync(leaderboardPage);
        }

        private async void OnAiCoachTapped(object sender, EventArgs e)
        {
            var aiCoachPage = Handler?.MauiContext?.Services.GetService<MauiApp8.Pages.AiCoachPage>();
            if (aiCoachPage != null)
                await Navigation.PushAsync(aiCoachPage);
        }
    }
}
