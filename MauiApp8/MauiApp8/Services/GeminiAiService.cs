using System.Net.Http.Json;
using System.Text.Json;

namespace MauiApp8.Services;

/// <summary>
/// Google Gemini API integration for AI guitar coaching.
/// Uses built-in API key — no user configuration needed.
/// Free tier: 15 requests/minute, 1M tokens/day.
/// </summary>
public class GeminiAiService : IAiService
{
    private readonly HttpClient _httpClient;

    // Models to try in order (fallback if primary model unavailable)
    private static readonly string[] Models = new[]
    {
        "gemini-2.0-flash",
        "gemini-2.0-flash-lite"
    };

    private const string BaseEndpoint = "https://generativelanguage.googleapis.com/v1beta/models";

    public GeminiAiService()
    {
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30)
        };

        // Clear any previously stored user key so the built-in key is always used
        try { Preferences.Remove("GeminiApiKey"); } catch { }
    }

    public bool IsConfigured => true;

    public async Task<string> SendMessageAsync(string userMessage, string systemContext = "")
    {
        var apiKey = AppConfig.GeminiApiKey;

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

            // Try each model until one works
            foreach (var model in Models)
            {
                var url = $"{BaseEndpoint}/{model}:generateContent?key={apiKey}";
                var response = await _httpClient.PostAsJsonAsync(url, requestBody);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var doc = JsonDocument.Parse(json);

                    var text = doc.RootElement
                        .GetProperty("candidates")[0]
                        .GetProperty("content")
                        .GetProperty("parts")[0]
                        .GetProperty("text")
                        .GetString();

                    return text ?? "I didn't get a response. Please try again.";
                }

                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Gemini API error ({model}, {response.StatusCode}): {error}");

                // If it's a 404 (model not found), try next model
                // For other errors (rate limit, auth), don't retry with different model
                if (response.StatusCode != System.Net.HttpStatusCode.NotFound)
                    break;
            }

            return "Sorry, I couldn't process your request right now. Please try again later.";
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
}
