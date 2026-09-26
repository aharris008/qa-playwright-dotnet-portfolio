namespace QaPlaywrightPortfolio.Tests.Configuration;

public static class TestSettings
{
    public const string UiBaseUrlEnvironmentVariable = "QA_UI_BASE_URL";

    private const string DefaultUiBaseUrl = "https://demo.playwright.dev/todomvc/";

    public static string UiBaseUrl => GetAbsoluteHttpUrl(
        UiBaseUrlEnvironmentVariable,
        DefaultUiBaseUrl);

    private static string GetAbsoluteHttpUrl(string environmentVariable, string defaultValue)
    {
        var configuredValue = Environment.GetEnvironmentVariable(environmentVariable);
        var value = string.IsNullOrWhiteSpace(configuredValue)
            ? defaultValue
            : configuredValue.Trim();

        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new InvalidOperationException(
                $"{environmentVariable} must be an absolute HTTP or HTTPS URL.");
        }

        return uri.AbsoluteUri;
    }
}



