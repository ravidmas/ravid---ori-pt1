using MauiApp8.Services;

namespace MauiApp8
{
    public partial class App : Application
    {
        public App(AchievementService achievementService)
        {
            InitializeComponent();

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
            return new Window(new AppShell());
        }
    }
}
