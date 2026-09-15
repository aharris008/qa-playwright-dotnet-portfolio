namespace QaPlaywrightPortfolio.Tests.Configuration;

public static class TestSettings
{
    public const string UiBaseUrlEnvironmentVariable = "QA_UI_BASE_URL";
    public const string ApiBaseUrlEnvironmentVariable = "QA_API_BASE_URL";

    private const string DefaultUiBaseUrl = "https://demo.playwright.dev/todomvc/";
    private const string DefaultApiBaseUrl = "https://jsonplaceholder.typicode.com/";

    public static string UiBaseUrl => GetAbsoluteHttpUrl(
        UiBaseUrlEnvironmentVariable,
        DefaultUiBaseUrl);

    public static string ApiBaseUrl => GetAbsoluteHttpUrl(
        ApiBaseUrlEnvironmentVariable,
        DefaultApiBaseUrl);

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
