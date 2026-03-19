using System.Net.Http.Json;
using System.Text.Json;

namespace MauiApp8.Services;

/// <summary>
/// Google Gemini API integration for AI guitar coaching.
/// Free tier: 15 requests/minute, 1M tokens/day.
/// </summary>
public class GeminiAiService : IAiService
{
    private readonly HttpClient _httpClient;
    private const string GeminiEndpoint = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent";

    public GeminiAiService()
    {
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30)
        };
    }

    public bool IsConfigured => !string.IsNullOrEmpty(GetApiKey());

    public async Task<string> SendMessageAsync(string userMessage, string systemContext = "")
    {
        var apiKey = GetApiKey();
        if (string.IsNullOrEmpty(apiKey))
            return "AI Coach is not configured yet. Please add your Gemini API key in Settings.";

        try
        {
            var systemPrompt = string.IsNullOrEmpty(systemContext)
                ? "You are a friendly and encouraging guitar teacher assistant for the RD Strings app. Give concise, practical guitar advice. Keep responses under 150 words."
                : systemContext;

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = $"{systemPrompt}\n\nUser: {userMessage}" }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.7,
                    maxOutputTokens = 300,
                    topP = 0.9
                }
            };

            var url = $"{GeminiEndpoint}?key={apiKey}";
            var response = await _httpClient.PostAsJsonAsync(url, requestBody);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Gemini API error: {error}");
                return "Sorry, I couldn't process your request right now. Please try again later.";
            }

            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);

            // Extract text from Gemini response
            var text = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            return text ?? "I didn't get a response. Please try again.";
        }
        catch (TaskCanceledException)
        {
            return "Request timed out. Please check your internet connection and try again.";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Gemini error: {ex.Message}");
            return "Something went wrong. Please try again later.";
        }
    }

    /// <summary>
    /// Get the API key from secure storage.
    /// </summary>
    private string GetApiKey()
    {
        try
        {
            return Preferences.Get("GeminiApiKey", "");
        }
        catch
        {
            return "";
        }
    }

    /// <summary>
    /// Save the API key to secure storage.
    /// </summary>
    public static void SetApiKey(string apiKey)
    {
        Preferences.Set("GeminiApiKey", apiKey);
    }
}
