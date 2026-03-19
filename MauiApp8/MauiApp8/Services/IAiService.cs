namespace MauiApp8.Services;

/// <summary>
/// Interface for AI chat/coaching functionality.
/// </summary>
public interface IAiService
{
    /// <summary>
    /// Send a message to the AI coach and get a response.
    /// </summary>
    Task<string> SendMessageAsync(string userMessage, string systemContext = "");

    /// <summary>
    /// Check if the AI service is configured with a valid API key.
    /// </summary>
    bool IsConfigured { get; }
}
