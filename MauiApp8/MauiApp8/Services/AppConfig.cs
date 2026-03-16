namespace MauiApp8.Services;

/// <summary>
/// Centralized application configuration.
/// Provides platform-aware settings (e.g. Android emulator vs localhost).
/// </summary>
public static class AppConfig
{
    /// <summary>
    /// The base URL for the chord API server.
    /// On Android emulator, 10.0.2.2 maps to the host machine's localhost.
    /// </summary>
    public static string ApiBaseUrl =>
        DeviceInfo.Platform == DevicePlatform.Android
            ? "http://10.0.2.2:5174"
            : "http://localhost:5174";

    /// <summary>
    /// HTTP request timeout in seconds
    /// </summary>
    public static int HttpTimeoutSeconds => 10;
}
