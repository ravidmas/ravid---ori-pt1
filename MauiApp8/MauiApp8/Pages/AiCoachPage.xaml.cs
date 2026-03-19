using MauiApp8.Data;
using MauiApp8.Services;

namespace MauiApp8.Pages;

public partial class AiCoachPage : ContentPage
{
    private readonly IAiService _aiService;
    private readonly AppDatabase _database;
    private bool _isWaitingForResponse;

    public AiCoachPage(IAiService aiService, AppDatabase database)
    {
        InitializeComponent();
        _aiService = aiService;
        _database = database;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // API key is built-in, banner hidden by default
        ApiKeyBanner.IsVisible = false;
    }

    private async void OnSendButtonClicked(object sender, EventArgs e)
    {
        await SendMessage();
    }

    private async void OnSendMessage(object sender, EventArgs e)
    {
        await SendMessage();
    }

    private async Task SendMessage()
    {
        var message = MessageEntry.Text?.Trim();
        if (string.IsNullOrEmpty(message) || _isWaitingForResponse)
            return;

        MessageEntry.Text = "";
        AddUserBubble(message);
        await SendToAi(message);
    }

    private async Task SendToAi(string message)
    {
        _isWaitingForResponse = true;

        // Add typing indicator
        var typingFrame = new Frame
        {
            BackgroundColor = (Color)Application.Current!.Resources["AppCream"],
            CornerRadius = 15,
            Padding = new Thickness(15),
            HasShadow = false,
            HorizontalOptions = LayoutOptions.Start,
            MaximumWidthRequest = 300
        };
        typingFrame.Content = new Label
        {
            Text = "Thinking...",
            FontSize = 14,
            TextColor = (Color)Application.Current!.Resources["Gray400"],
            FontAttributes = FontAttributes.Italic
        };
        ChatContainer.Children.Add(typingFrame);
        await ScrollToBottom();

        try
        {
            // Build system context with user's progress data
            var systemContext = await BuildSystemContextAsync();
            var response = await _aiService.SendMessageAsync(message, systemContext);

            // Remove typing indicator and add response
            ChatContainer.Children.Remove(typingFrame);
            AddAiBubble(response);
        }
        catch (Exception ex)
        {
            ChatContainer.Children.Remove(typingFrame);
            AddAiBubble("Sorry, I couldn't process that request. Please try again.");
            Console.WriteLine($"AI error: {ex.Message}");
        }
        finally
        {
            _isWaitingForResponse = false;
        }
    }

    private async Task<string> BuildSystemContextAsync()
    {
        try
        {
            var stats = await _database.GetUserStatsAsync();
            var mastery = await _database.GetAllChordMasteryAsync();
            var weakChords = await _database.GetWeakChordsAsync();

            var masteredChords = mastery.Where(m => m.IsMastered).Select(m => m.ChordName).ToList();
            var weakChordNames = weakChords.Select(m => $"{m.ChordName} ({m.MasteryPercent:F0}%)").ToList();

            return $@"You are a friendly and encouraging guitar teacher assistant for the RD Strings app.
The student has completed {stats.TotalSessions} practice sessions with {stats.OverallAccuracy:F0}% accuracy.
Mastered chords: {(masteredChords.Any() ? string.Join(", ", masteredChords) : "none yet")}.
Weak chords needing practice: {(weakChordNames.Any() ? string.Join(", ", weakChordNames) : "none identified yet")}.
Current streak: {stats.CurrentStreak} days.
Give concise, encouraging, practical guitar advice. Keep responses under 150 words.";
        }
        catch
        {
            return "You are a friendly and encouraging guitar teacher assistant. Give concise, practical guitar advice. Keep responses under 150 words.";
        }
    }

    private void AddUserBubble(string text)
    {
        var frame = new Frame
        {
            BackgroundColor = (Color)Application.Current!.Resources["AppBrown"],
            CornerRadius = 15,
            Padding = new Thickness(15),
            HasShadow = false,
            HorizontalOptions = LayoutOptions.End,
            MaximumWidthRequest = 280
        };
        frame.Content = new Label
        {
            Text = text,
            FontSize = 14,
            TextColor = Colors.White,
            LineHeight = 1.3
        };
        ChatContainer.Children.Add(frame);
        _ = ScrollToBottom();
    }

    private void AddAiBubble(string text)
    {
        var frame = new Frame
        {
            BackgroundColor = (Color)Application.Current!.Resources["AppCream"],
            CornerRadius = 15,
            Padding = new Thickness(15),
            HasShadow = false,
            HorizontalOptions = LayoutOptions.Start,
            MaximumWidthRequest = 300
        };
        frame.Content = new Label
        {
            Text = text,
            FontSize = 14,
            TextColor = (Color)Application.Current!.Resources["AppDarkBrown"],
            LineHeight = 1.3
        };
        ChatContainer.Children.Add(frame);
        _ = ScrollToBottom();
    }

    private async Task ScrollToBottom()
    {
        await Task.Delay(100);
        await ChatScrollView.ScrollToAsync(0, ChatContainer.Height, true);
    }

    // Quick ask buttons
    private async void OnQuickAsk1(object sender, EventArgs e)
    {
        MessageEntry.Text = "";
        AddUserBubble("What should I practice today?");
        await SendToAi("Based on my progress data, what should I practice today? Give me a specific 10-minute practice plan.");
    }

    private async void OnQuickAsk2(object sender, EventArgs e)
    {
        MessageEntry.Text = "";
        AddUserBubble("Tips for beginners");
        await SendToAi("I'm a beginner learning guitar. What are your top 5 tips for someone just starting out?");
    }

    private async void OnQuickAsk3(object sender, EventArgs e)
    {
        MessageEntry.Text = "";
        AddUserBubble("How to switch chords faster?");
        await SendToAi("How can I switch between chords faster and more smoothly? Give me practical exercises.");
    }

    private void OnToggleApiKey(object sender, EventArgs e)
    {
        ApiKeyBanner.IsVisible = !ApiKeyBanner.IsVisible;
    }

    private void OnSaveApiKey(object sender, EventArgs e)
    {
        var key = ApiKeyEntry.Text?.Trim();
        if (!string.IsNullOrEmpty(key))
        {
            GeminiAiService.SetApiKey(key);
            ApiKeyBanner.IsVisible = false;
            AddAiBubble("API key saved! I'm ready to help you learn guitar. Ask me anything!");
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
