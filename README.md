# Playwright .NET QA Automation Portfolio

[![Playwright Tests](https://github.com/aharris008/qa-playwright-dotnet-portfolio/actions/workflows/playwright.yml/badge.svg)](https://github.com/aharris008/qa-playwright-dotnet-portfolio/actions/workflows/playwright.yml)

This repository demonstrates practical QA automation with Playwright for .NET, C#, and NUnit: readable test intent, stable UI locators, web-first assertions, HTTP/API validation, focused abstractions, deterministic test design, failure diagnostics, and repeatable GitHub Actions execution.
Scenarios use a public UI demonstration service and a local demo API with synthetic test data. The repository contains no employer code, proprietary workflows, credentials, or confidential data.

## What is covered

The current suite contains four independent Chromium tests against Playwright's public TodoMVC application and three browser-free API tests against a local HTTP fixture.

| Scenario | QA purpose | Categories |
| --- | --- | --- |
| Add a todo | Core happy path, visible result, and count update | `UI`, `Smoke` |
| Complete one of two todos | State transition, checked state, and remaining-count validation | `UI`, `Regression` |
| Filter completed todos | Multi-step workflow and visible/hidden result validation | `UI`, `Regression` |
| Submit an empty todo | Boundary/negative validation that no record is created | `UI`, `Boundary`, `Negative` |
| Retrieve a known todo | HTTP status, JSON content type, and typed field validation | `API` |
| Filter todos by user | Exact matching IDs and exclusion of another user's data | `API` |
| Retrieve an unknown todo | HTTP 404 and a JSON error contract without a success payload | `API` |

These examples emphasize patterns that transfer to regression reliability work: isolated tests, intentional coverage, maintainable interaction boundaries, deterministic assertions, CI execution, and useful artifacts when a failure must be investigated.

## Technologies

- .NET 8 and C#
- Microsoft Playwright for .NET 1.62
- NUnit 4
- NUnit test adapter and analyzers
- GitHub Actions on `ubuntu-latest`
- Chromium

Package versions are pinned in the test project. The local API uses the ASP.NET Core shared framework, with no additional NuGet packages. Coverlet is intentionally omitted: line coverage of the small test harness and demo fixture would not measure coverage of the tested behaviors.

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
│   │   ├── LocalTodoApi.cs
│   │   └── UiTestBase.cs
│   ├── Models/
│   │   └── TodoResponse.cs
│   ├── Pages/
│   │   └── TodoPage.cs
│   ├── Tests/
│   │   ├── API/
│   │   │   └── TodoApiTests.cs
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
| `QA_API_BASE_URL` | `https://jsonplaceholder.typicode.com/` | Reserved for future external API checks; unused by this suite |

`QA_UI_BASE_URL` validates an absolute HTTP or HTTPS URL when accessed. No `.env` loader is used; set the override in the shell or CI environment that starts the test process. Local API tests use their fixture's loopback address.

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

# Run only the local API tests (no browser installation required)
dotnet test .\QaPlaywrightPortfolio.sln --filter "TestCategory=API"
```

## Local API coverage

`LocalTodoApi` starts an ASP.NET Core/Kestrel server on `127.0.0.1` with an OS-assigned port and fixed, read-only data. It implements lookup and user filtering, plus a `404` JSON response containing only `code: "todo_not_found"`. Startup is awaited, and the server is disposed after the fixture. Each test creates and disposes its own Playwright `IAPIRequestContext`; no browser, Docker, database, or external API is required.

Server seed data is separate from the client response model and test expectations. Mixed users make the filter test detect ignored filters, missing results, and unwanted results. These tests demonstrate HTTP and contract validation against a controlled demo, not verification of JSONPlaceholder or a production service. The existing UI tests still require the public TodoMVC service.

## CI behavior

`.github/workflows/playwright.yml` runs for pushes and pull requests targeting `main`. The job uses minimal read-only repository permissions and does not require secrets. It:

1. restores and builds the solution with .NET 8 in Release configuration;
2. installs Chromium and its required Ubuntu dependencies;
3. runs all seven UI and API tests with the same NUnit command and generates a TRX result file;
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

- UI coverage targets one public TodoMVC application in Chromium; its four tests are not a comprehensive regression pack.
- Three local API tests exercise a controlled, read-only demo. The external API URL is configured but not exercised.
- No database is required.
- Cross-browser, accessibility, external API, and UI/API data-consistency coverage are not yet implemented.

The portfolio stays compact so that each test demonstrates a distinct, practical QA technique. Future additions may include UI/API data-validation examples against a shared application.

## License

This project is available under the [MIT License](LICENSE).

