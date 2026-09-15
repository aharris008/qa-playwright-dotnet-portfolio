# Playwright .NET QA Automation Portfolio

[![Playwright Tests](https://github.com/aharris008/qa-playwright-dotnet-portfolio/actions/workflows/playwright.yml/badge.svg)](https://github.com/aharris008/qa-playwright-dotnet-portfolio/actions/workflows/playwright.yml)

This repository demonstrates practical browser-test design with Playwright for .NET, C#, and NUnit: readable test intent, stable user-facing locators, web-first assertions, focused page objects, environment-aware configuration, failure diagnostics, and repeatable GitHub Actions execution.

All scenarios use public demonstration services and synthetic test data. The repository contains no employer code, proprietary workflows, credentials, or confidential data.

## What is covered

The current suite contains four independent Chromium tests against Playwright's public TodoMVC application.

| Scenario | QA purpose | Categories |
| --- | --- | --- |
| Add a todo | Core happy path, visible result, and count update | `UI`, `Smoke` |
| Complete one of two todos | State transition, checked state, and remaining-count validation | `UI`, `Regression` |
| Filter completed todos | Multi-step workflow and visible/hidden result validation | `UI`, `Regression` |
| Submit an empty todo | Boundary/negative validation that no record is created | `UI`, `Boundary`, `Negative` |

These examples emphasize patterns that transfer to regression reliability work: isolated tests, intentional coverage, maintainable interaction boundaries, deterministic assertions, CI execution, and useful artifacts when a failure must be investigated.

## Technologies

- .NET 8 and C#
- Microsoft Playwright for .NET 1.62
- NUnit 4
- NUnit test adapter and analyzers
- GitHub Actions on `ubuntu-latest`
- Chromium

Package versions are pinned in the test project. Coverlet is intentionally omitted because the current tests exercise an external public application; measuring line coverage of the small test harness would not provide meaningful quality information.

## Repository structure

```text
qa-playwright-dotnet-portfolio/
├── .github/
│   └── workflows/
│       └── playwright.yml
├── QaPlaywrightPortfolio.Tests/
│   ├── Configuration/
│   │   └── TestSettings.cs
│   ├── Fixtures/
│   │   └── UiTestBase.cs
│   ├── Pages/
│   │   └── TodoPage.cs
│   ├── Tests/
│   │   └── UI/
│   │       └── TodoTests.cs
│   └── QaPlaywrightPortfolio.Tests.csproj
├── .editorconfig
├── .gitignore
├── LICENSE
├── QaPlaywrightPortfolio.sln
└── README.md
```

## Architecture and design decisions

- **Tests describe behavior.** Assertions stay in the test class so each scenario reads as an executable requirement.
- **A focused page object owns interactions.** `TodoPage` centralizes navigation and TodoMVC controls without creating a broad framework or deep inheritance hierarchy.
- **Locators reflect the interface.** The suite prefers placeholder, role, exact visible text, and the demo application's stable test IDs over fragile CSS or DOM paths.
- **Assertions wait for browser state.** Playwright's web-first assertions replace fixed sleeps and manual polling.
- **Tests are independent.** Playwright's NUnit fixture provides a fresh browser context and page for each test, avoiding shared-state coupling.
- **One small base fixture adds diagnostics.** `UiTestBase` starts a trace for each UI test and retains a full-page screenshot and trace only when that test fails.
- **Configuration stays lightweight.** Public defaults make the suite runnable without secrets, while process-level environment variables support alternate environments.

## Configuration

| Environment variable | Default | Purpose |
| --- | --- | --- |
| `QA_UI_BASE_URL` | `https://demo.playwright.dev/todomvc/` | UI application under test |
| `QA_API_BASE_URL` | `https://jsonplaceholder.typicode.com/` | Reserved for the next API-test milestone |

Both values must be absolute HTTP or HTTPS URLs. No `.env` loader is used; set overrides in the shell or CI environment that starts the test process.

PowerShell example:

```powershell
$env:QA_UI_BASE_URL = "https://demo.playwright.dev/todomvc/"
$env:QA_API_BASE_URL = "https://jsonplaceholder.typicode.com/"
dotnet test .\QaPlaywrightPortfolio.sln
```

Bash, zsh, or another POSIX-compatible shell:

```bash
QA_UI_BASE_URL="https://demo.playwright.dev/todomvc/" \
QA_API_BASE_URL="https://jsonplaceholder.typicode.com/" \
dotnet test ./QaPlaywrightPortfolio.sln
```

## Local setup and execution

### Prerequisites

- .NET 8 SDK
- PowerShell 7 (`pwsh`) on macOS or Linux
- Internet access to install Chromium and reach the configured public test application

### Windows PowerShell

From the repository root:

```powershell
dotnet restore .\QaPlaywrightPortfolio.sln
dotnet build .\QaPlaywrightPortfolio.sln --configuration Debug --no-restore
powershell -ExecutionPolicy Bypass -File .\QaPlaywrightPortfolio.Tests\bin\Debug\net8.0\playwright.ps1 install chromium
dotnet test .\QaPlaywrightPortfolio.sln --configuration Debug --no-build
```

If your execution policy already permits local scripts, the browser install can be shortened to:

```powershell
.\QaPlaywrightPortfolio.Tests\bin\Debug\net8.0\playwright.ps1 install chromium
```

### Platform-neutral .NET commands

Restore, build, and test commands are the same on any supported platform:

```bash
dotnet restore ./QaPlaywrightPortfolio.sln
dotnet build ./QaPlaywrightPortfolio.sln --configuration Debug --no-restore
pwsh ./QaPlaywrightPortfolio.Tests/bin/Debug/net8.0/playwright.ps1 install chromium
dotnet test ./QaPlaywrightPortfolio.sln --configuration Debug --no-build
```

The browser installation is normally needed only for initial setup or after a Playwright package update.

## Run selected categories

NUnit categories are exposed through the standard `TestCategory` filter.

```powershell
# Fast confidence check
dotnet test .\QaPlaywrightPortfolio.sln --filter "TestCategory=Smoke"

# All UI coverage
dotnet test .\QaPlaywrightPortfolio.sln --filter "TestCategory=UI"

# Boundary and negative coverage
dotnet test .\QaPlaywrightPortfolio.sln --filter "TestCategory=Boundary|TestCategory=Negative"

# Exclude UI tests when later API tests are added
dotnet test .\QaPlaywrightPortfolio.sln --filter "TestCategory!=UI"
```

## CI behavior

`.github/workflows/playwright.yml` runs for pushes and pull requests targeting `main`. The job uses minimal read-only repository permissions and does not require secrets. It:

1. restores and builds the solution with .NET 8 in Release configuration;
2. installs Chromium and its required Ubuntu dependencies;
3. runs the NUnit suite and generates a TRX result file;
4. uploads TRX results even when tests fail; and
5. uploads Playwright screenshots and traces when a failure occurs.

## Failure diagnostics

On a UI-test failure, the suite writes a timestamped full-page PNG and Playwright trace ZIP beneath `TestResults/playwright/`. These generated files are ignored by Git and retained by CI for 14 days.

Open a saved trace locally with the Playwright CLI produced by the build:

```powershell
powershell -ExecutionPolicy Bypass -File .\QaPlaywrightPortfolio.Tests\bin\Debug\net8.0\playwright.ps1 show-trace .\TestResults\playwright\<trace-file>.zip
```

Diagnostics are best-effort: an artifact-capture problem is logged without hiding the original test failure.

## Current limitations and next scope

- Coverage currently targets one public TodoMVC UI in Chromium.
- The suite intentionally remains at four UI tests for this increment; it is not a comprehensive TodoMVC regression pack.
- The API URL is configured but not exercised yet.
- No database is required by the public demo.
- Cross-browser, accessibility, API, and UI/API data-consistency coverage are not yet implemented.

The next increments will add Playwright `IAPIRequestContext` tests, typed response models, and public UI/API data-validation examples. The longer-term target is a compact 10–20-test portfolio in which every test demonstrates a distinct, practical QA technique.

## License

This project is available under the [MIT License](LICENSE).
