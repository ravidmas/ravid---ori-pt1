using MauiApp8.Data;

namespace MauiApp8.Pages;

public partial class SignUpPage : ContentPage
{
    private readonly AppDatabase _database;

    public SignUpPage(AppDatabase database)
    {
        InitializeComponent();
        _database = database;
    }

    private async void OnSignUpClicked(object sender, EventArgs e)
    {
        var name = NameEntry.Text?.Trim();
        var email = EmailEntry.Text?.Trim();
        var password = PasswordEntry.Text;
        var confirmPassword = ConfirmPasswordEntry.Text;

        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ShowError("Please fill in all fields.");
            return;
        }

        if (password.Length < 6)
        {
            ShowError("Password must be at least 6 characters.");
            return;
        }

        if (password != confirmPassword)
        {
            ShowError("Passwords do not match.");
            return;
        }

        var passwordHash = LoginPage.HashPassword(password);
        var success = await _database.RegisterUserAsync(name, email, passwordHash);

        if (!success)
        {
            ShowError("An account with this email already exists.");
            return;
        }

        // Auto-login after registration
        var user = await _database.AuthenticateUserAsync(email, passwordHash);
        if (user != null)
        {
            Preferences.Set("LoggedInUserId", user.UserId);
            Preferences.Set("UserDisplayName", user.DisplayName);
            Preferences.Set("UserEmail", user.Email);
            Preferences.Set("JoinDate", user.CreatedAt.ToString("yyyy-MM-dd"));

            var services = Handler?.MauiContext?.Services
                ?? Application.Current?.Handler?.MauiContext?.Services;
            var mainPage = services?.GetService<MainPage>() ?? new MainPage();

            if (Application.Current != null)
                Application.Current.Windows[0].Page = new NavigationPage(mainPage);
        }
    }

    private async void OnBackToLoginTapped(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private void ShowError(string message)
    {
        ErrorLabel.Text = message;
        ErrorLabel.IsVisible = true;
    }
}
