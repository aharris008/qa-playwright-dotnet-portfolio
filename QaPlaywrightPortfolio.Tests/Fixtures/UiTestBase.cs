using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework.Interfaces;

namespace QaPlaywrightPortfolio.Tests.Fixtures;

public abstract class UiTestBase : PageTest
{
    private bool _tracingStarted;

    [SetUp]
    public async Task StartTrace()
    {
        await Context.Tracing.StartAsync(new TracingStartOptions
        {
            Screenshots = true,
            Snapshots = true,
            Sources = true,
        });

        _tracingStarted = true;
    }

    [TearDown]
    public async Task CaptureFailureArtifacts()
    {
        if (!_tracingStarted)
        {
            return;
        }

        var failed = TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Failed;

        if (!failed)
        {
            await Context.Tracing.StopAsync();
            return;
        }

        var artifactDirectory = Path.Combine(
            TestContext.CurrentContext.WorkDirectory,
            "TestResults",
            "playwright");
        Directory.CreateDirectory(artifactDirectory);

        var testName = SanitizeFileName(TestContext.CurrentContext.Test.Name);
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmssfff");
        var artifactName = $"{testName}-{timestamp}";

        try
        {
            if (!Page.IsClosed)
            {
                await Page.ScreenshotAsync(new PageScreenshotOptions
                {
                    FullPage = true,
                    Path = Path.Combine(artifactDirectory, $"{artifactName}.png"),
                });
            }
        }
        catch (Exception exception)
        {
            TestContext.Progress.WriteLine(
                $"Unable to capture failure screenshot: {exception.Message}");
        }

        try
        {
            await Context.Tracing.StopAsync(new TracingStopOptions
            {
                Path = Path.Combine(artifactDirectory, $"{artifactName}-trace.zip"),
            });
        }
        catch (Exception exception)
        {
            TestContext.Progress.WriteLine(
                $"Unable to save Playwright trace: {exception.Message}");
        }
    }

    private static string SanitizeFileName(string value)
    {
        var invalidCharacters = Path.GetInvalidFileNameChars();
        return string.Concat(value.Select(character =>
            invalidCharacters.Contains(character) ? '_' : character));
    }
}
