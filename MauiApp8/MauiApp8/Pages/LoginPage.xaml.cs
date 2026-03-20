using System.Security.Cryptography;
using System.Text;
using MauiApp8.Data;

namespace MauiApp8.Pages;

public partial class LoginPage : ContentPage
{
    private readonly AppDatabase _database;

    public LoginPage(AppDatabase database)
    {
        InitializeComponent();
        _database = database;
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        var email = EmailEntry.Text?.Trim();
        var password = PasswordEntry.Text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ShowError("Please enter both email and password.");
            return;
        }

        try
        {
            var passwordHash = HashPassword(password);
            var user = await _database.AuthenticateUserAsync(email, passwordHash);

            if (user == null)
            {
                ShowError("Invalid email or password.");
                return;
            }

            // Save logged-in user info
            Preferences.Set("LoggedInUserId", user.UserId);
            Preferences.Set("UserDisplayName", user.DisplayName);
            Preferences.Set("UserEmail", user.Email);
            Preferences.Set("JoinDate", user.CreatedAt.ToString("yyyy-MM-dd"));

            // Navigate to main app using DI
            var services = Handler?.MauiContext?.Services
                ?? Application.Current?.Handler?.MauiContext?.Services;
            var mainPage = services?.GetService<MainPage>() ?? new MainPage();

            if (Application.Current != null)
                Application.Current.Windows[0].Page = new NavigationPage(mainPage);
        }
        catch (Exception ex)
        {
            ShowError($"Login error: {ex.Message}");
            Console.WriteLine($"Login error: {ex}");
        }
    }

    private async void OnSignUpTapped(object sender, EventArgs e)
    {
        var signUpPage = Handler?.MauiContext?.Services.GetService<SignUpPage>();
        if (signUpPage != null)
            await Navigation.PushAsync(signUpPage);
    }

    private void ShowError(string message)
    {
        ErrorLabel.Text = message;
        ErrorLabel.IsVisible = true;
    }

    public static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }
}
