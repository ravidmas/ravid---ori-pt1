using MauiApp8.Pages;
using MauiApp8.Services;

namespace MauiApp8
{
    public partial class App : Application
    {
        private readonly IServiceProvider _services;

        public App(AchievementService achievementService, IServiceProvider services)
        {
            InitializeComponent();
            _services = services;

            // Initialize achievements in database on startup
            Task.Run(async () =>
            {
                try
                {
                    await achievementService.InitializeAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Achievement init error: {ex.Message}");
                }
            });
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var isLoggedIn = !string.IsNullOrEmpty(Preferences.Get("LoggedInUserId", ""));

            if (isLoggedIn)
            {
                return new Window(new NavigationPage(
                    _services.GetService<MainPage>() ?? new MainPage()));
            }
            else
            {
                return new Window(new NavigationPage(
                    _services.GetService<LoginPage>()
                    ?? new LoginPage(_services.GetRequiredService<MauiApp8.Data.AppDatabase>())));
            }
        }
    }
}
