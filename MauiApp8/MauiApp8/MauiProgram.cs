using Microsoft.Extensions.Logging;
using Plugin.Maui.Audio;
using MauiApp8.Data;
using MauiApp8.Services;
using MauiApp8.ViewModels;
using MauiApp8.Pages;

namespace MauiApp8
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Register database
            builder.Services.AddSingleton<AppDatabase>();

            // Register services
            builder.Services.AddSingleton<IChordService, ChordService>();
            builder.Services.AddSingleton(AudioManager.Current);
            builder.Services.AddSingleton<IAudioService, AudioService>();
            builder.Services.AddSingleton<IChordDetectionService, ChordDetectionService>();
            builder.Services.AddSingleton<AchievementService>();
            builder.Services.AddSingleton<LeaderboardService>();
            builder.Services.AddSingleton<IAiService, GeminiAiService>();

            // Register ViewModels
            builder.Services.AddTransient<LearnChordsViewModel>();
            builder.Services.AddTransient<PracticeViewModel>();

            // Register pages
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<SignUpPage>();
            builder.Services.AddTransient<LearnChordsPage>();
            builder.Services.AddTransient<PracticePage>();
            builder.Services.AddTransient<LessonsPage>();
            builder.Services.AddTransient<ProfilePage>();
            builder.Services.AddTransient<ProgressPage>();
            builder.Services.AddTransient<TrophiesPage>();
            builder.Services.AddTransient<LeaderboardPage>();
            builder.Services.AddTransient<AiCoachPage>();
            builder.Services.AddSingleton<MainPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
