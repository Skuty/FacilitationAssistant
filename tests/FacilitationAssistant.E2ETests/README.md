# Facilitation Assistant - E2E Tests

End-to-End tests for the Facilitation Assistant application using Playwright for .NET with xUnit.

## Overview

This test suite provides comprehensive E2E coverage of the Facilitation Assistant application, validating critical user journeys from the perspective of both facilitators and attendees. Tests are organized by priority (P0-P3) and feature area.

## Prerequisites

- .NET 9.0 SDK
- Playwright browsers (automatically installed on first run)

## Quick Start

### 1. Install Dependencies

```powershell
cd tests/FacilitationAssistant.E2ETests
dotnet restore
```

### 2. Install Playwright Browsers

```powershell
dotnet build
pwsh bin/Debug/net9.0/playwright.ps1 install
```

Or on first test run, browsers will be installed automatically.

### 3. Run Tests

**Run all tests:**
```powershell
dotnet test
```

**Run tests by priority:**
```powershell
# P0 (Critical) tests only
dotnet test --filter "Priority=P0"

# P1 (High) tests only
dotnet test --filter "Priority=P1"
```

**Run tests by feature:**
```powershell
# Meeting Creation tests
dotnet test --filter "Feature=MeetingCreation"

# Agenda Management tests
dotnet test --filter "Feature=AgendaManagement"

# Polling System tests
dotnet test --filter "Feature=PollingSystem"
```

**Run tests with detailed output:**
```powershell
dotnet test --logger "console;verbosity=detailed"
```

## Project Structure

```
FacilitationAssistant.E2ETests/
├── Fixtures/
│   ├── PlaywrightFixture.cs          # Browser lifecycle management
│   ├── WebApplicationFixture.cs      # ASP.NET app hosting
│   └── TestSettings.cs                # Configuration model
├── PageObjects/
│   ├── BasePage.cs                    # Base page with common methods
│   ├── HomePage.cs                    # Home/meeting creation page
│   ├── FacilitatorPage.cs             # Facilitator view
│   ├── AttendeePage.cs                # Attendee view
│   └── SummaryPage.cs                 # Meeting summary page
├── Tests/
│   ├── MeetingCreationTests.cs        # P0: Meeting & link tests
│   ├── AgendaManagementTests.cs       # P0: Agenda & timer tests
│   ├── PollingSystemTests.cs          # P0: Questions & polls tests
│   ├── RealTimeSyncTests.cs           # P0: SignalR sync tests
│   ├── ConcernsFeedbackTests.cs       # P1: Concerns tests
│   └── MeetingSummaryTests.cs         # P1: Summary tests
├── Utilities/
│   └── BrowserHelpers.cs              # Helper methods
├── appsettings.Test.json              # Test configuration
├── FacilitationAssistant.E2ETests.csproj
└── README.md                          # This file
```

## Configuration

Tests are configured via `appsettings.Test.json`:

```json
{
  "TestSettings": {
    "BaseUrl": "http://localhost:5000",      // Application URL
    "BrowserType": "chromium",               // chromium, firefox, or webkit
    "Headless": true,                        // Run browser headlessly
    "SlowMo": 0,                             // Slow down operations (ms)
    "Timeout": 30000,                        // Default timeout (ms)
    "VideoOnFailure": true,                  // Record video on failure
    "ScreenshotOnFailure": true,             // Take screenshot on failure
    "TraceOnFailure": true                   // Capture trace on failure
  }
}
```

### Environment Variables

You can override settings using environment variables:
- `TEST_BASE_URL` - Override the base URL
- `TEST_HEADLESS` - Set to `false` to see browser UI during test runs
- `TEST_BROWSER` - Set to `firefox` or `webkit` to use different browsers

## Test Categories

### P0 - Critical (Must Pass for Release)
Tests that validate core functionality without which the application is unusable.

**Test Classes:**
- `MeetingCreationTests` - Meeting creation, link generation, access control
- `AgendaManagementTests` - Stage management, timers, real-time sync
- `PollingSystemTests` - Question creation, triggering, responses
- `RealTimeSyncTests` - SignalR connection, state synchronization

**Run P0 tests:**
```powershell
dotnet test --filter "Priority=P0"
```

### P1 - High (Should Pass for Release)
Tests that validate important features that significantly impact user experience.

**Test Classes:**
- `ConcernsFeedbackTests` - Concerns, voting, acknowledgment
- `MeetingSummaryTests` - Meeting end, summary generation

**Run P1 tests:**
```powershell
dotnet test --filter "Priority=P1"
```

### P2 - Medium (Nice to Have)
Tests for edge cases and less critical features (to be implemented).

### P3 - Low (Enhancement)
Tests for polish features, accessibility, and performance (to be implemented).

## Page Object Model

Tests use the Page Object Model pattern to improve maintainability and readability.

### Example Usage

```csharp
// Create a page object
var homePage = new HomePage(_page, _baseUrl);

// Navigate to home
await homePage.NavigateAsync();

// Interact with page
await homePage.ClickCreateMeetingAsync();
var facilitatorLink = await homePage.GetFacilitatorLinkAsync();

// Assert
facilitatorLink.Should().NotBeNullOrEmpty();
```

### Available Page Objects

- **HomePage** - Meeting creation and link management
- **FacilitatorPage** - Full facilitator controls (stages, questions, messages, notes, concerns)
- **AttendeePage** - Attendee view (questions, concerns, notes, messages)
- **SummaryPage** - Post-meeting summary view

## Writing New Tests

### 1. Create Test Class

```csharp
using FacilitationAssistant.E2ETests.Fixtures;
using FacilitationAssistant.E2ETests.PageObjects;

namespace FacilitationAssistant.E2ETests.Tests;

[Collection("E2E")]
[Trait("Priority", "P1")]
[Trait("Feature", "YourFeature")]
public class YourFeatureTests : IAsyncLifetime
{
    private readonly PlaywrightFixture _playwrightFixture;
    private readonly WebApplicationFixture _webAppFixture;
    private IBrowserContext? _context;
    private IPage? _page;

    public YourFeatureTests(PlaywrightFixture playwrightFixture, 
                            WebApplicationFixture webAppFixture)
    {
        _playwrightFixture = playwrightFixture;
        _webAppFixture = webAppFixture;
    }

    public async Task InitializeAsync()
    {
        _context = await _playwrightFixture.CreateContextAsync(
            recordVideo: true, 
            recordTrace: true
        );
        _page = await _context.NewPageAsync();
    }

    public async Task DisposeAsync()
    {
        if (_page != null) await _page.CloseAsync();
        if (_context != null) await _context.CloseAsync();
    }

    [Fact]
    public async Task YourTest_ShouldDoSomething()
    {
        // Arrange
        var homePage = new HomePage(_page!, _playwrightFixture.Settings.BaseUrl);
        
        // Act
        await homePage.NavigateAsync();
        
        // Assert
        var title = await homePage.GetTitleAsync();
        title.Should().NotBeNullOrEmpty();
    }
}
```

### 2. Test Naming Convention

Format: `P{Priority}_{TestNumber}_{Scenario}_{ExpectedOutcome}`

Examples:
- `P0_001_CreateMeeting_ShouldGenerateUniqueFacilitatorAndAttendeeLinks`
- `P1_040_AttendeeRaisesPredefinedConcern_ShouldAppearInPanel`

### 3. Using FluentAssertions

All tests use FluentAssertions for readable test assertions:

```csharp
// Instead of:
Assert.True(result);
Assert.Equal(expected, actual);

// Use:
result.Should().BeTrue();
actual.Should().Be(expected);
facilitatorLink.Should().NotBeNullOrEmpty("because facilitator link must be generated");
```

## Debugging Tests

### Run Tests in Non-Headless Mode

Edit `appsettings.Test.json`:
```json
"Headless": false
```

Or set environment variable:
```powershell
$env:TEST_HEADLESS = "false"
dotnet test --filter "Priority=P0"
```

### Slow Down Test Execution

```json
"SlowMo": 100  // Slow down by 100ms per operation
```

### View Test Results

Test results are saved to:
- **Videos**: `test-results/videos/` (on failure if `VideoOnFailure=true`)
- **Screenshots**: `test-results/screenshots/` (on failure if `ScreenshotOnFailure=true`)
- **Traces**: `test-results/traces/` (on failure if `TraceOnFailure=true`)

### Use Playwright Inspector

Set the `PWDEBUG` environment variable to launch Playwright Inspector:

```powershell
$env:PWDEBUG = "1"
dotnet test --filter "FullyQualifiedName~YourSpecificTest"
```

This will pause test execution and allow you to step through and inspect the browser.

### Add Pause in Test

Temporarily pause test execution:

```csharp
await _page.PauseAsync();
```

## CI/CD Integration

### GitHub Actions Example

```yaml
name: E2E Tests

on: [pull_request]

jobs:
  e2e-tests:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '9.0.x'
      
      - name: Restore dependencies
        run: dotnet restore
      
      - name: Build
        run: dotnet build --no-restore
      
      - name: Install Playwright
        run: pwsh tests/FacilitationAssistant.E2ETests/bin/Debug/net9.0/playwright.ps1 install --with-deps
      
      - name: Run P0 Tests
        run: dotnet test tests/FacilitationAssistant.E2ETests --filter "Priority=P0" --logger "trx;LogFileName=test-results.trx"
      
      - name: Upload Test Results
        if: always()
        uses: actions/upload-artifact@v3
        with:
          name: test-results
          path: tests/FacilitationAssistant.E2ETests/TestResults/
```

### Recommended CI Strategy

- **On Pull Request**: Run P0 tests (critical path)
- **On Merge to Main**: Run P0 + P1 tests
- **Nightly**: Run all tests (P0, P1, P2, P3)

## Test Data Management

Tests create meeting data during execution. The In-Memory database is used by default for test isolation.

### Cleanup

Test data is automatically cleaned up between test classes. Each test class gets a fresh database context.

## Common Patterns

### Multi-User Tests (Facilitator + Attendees)

```csharp
// Create meeting
var homePage = new HomePage(_page!, _baseUrl);
await homePage.NavigateAsync();
await homePage.ClickCreateMeetingAsync();
var facilitatorLink = await homePage.GetFacilitatorLinkAsync();
var attendeeLink = await homePage.GetAttendeeLinkAsync();

// Setup facilitator
await _page!.GotoAsync(facilitatorLink!);
var facilitatorPage = new FacilitatorPage(_page, _baseUrl);
await facilitatorPage.StartMeetingAsync();

// Setup attendee in new page
var attendeePage = await _context!.NewPageAsync();
await attendeePage.GotoAsync(attendeeLink!);
var attendeePageObject = new AttendeePage(attendeePage, _baseUrl);
await attendeePageObject.WaitForMeetingStartAsync();

// Both can interact now
await facilitatorPage.StartStageAsync(0);
await Task.Delay(2000); // Wait for sync
var activeStage = await attendeePageObject.GetActiveStageTextAsync();

// Cleanup
await attendeePage.CloseAsync();
```

### Waiting for Real-Time Updates

```csharp
// Wait for stage sync with timeout
var synced = await attendeePageObject.WaitForStageSyncAsync("Stage Name", timeoutMs: 2000);
synced.Should().BeTrue();

// Or use custom condition
var success = await BrowserHelpers.WaitForConditionAsync(
    async () => await page.IsVisibleAsync(".some-element"),
    timeoutMs: 5000
);
```

### Taking Screenshots for Debugging

```csharp
// Manual screenshot
await _page.SaveScreenshotAsync("debug-screenshot");

// Or use page object
await homePage.TakeScreenshotAsync("after-meeting-creation");
```

## Known Limitations

1. **Headless Mode**: Some animations may not render correctly in headless mode
2. **Timing**: Real-time sync tests may be flaky on slow machines (configured with generous timeouts)
3. **Browser Support**: Primary testing on Chromium; Firefox and WebKit are secondary
4. **Mobile Emulation**: Mobile tests use emulation, not real devices

## Troubleshooting

### Tests Fail with "Element not found"

**Cause**: Selector may have changed or timing issue.

**Solution**: 
- Run in non-headless mode to see what's happening
- Add `await Task.Delay()` before assertion
- Update selector in Page Object

### "Browser not installed" Error

**Solution**:
```powershell
pwsh bin/Debug/net9.0/playwright.ps1 install
```

### Tests Timeout

**Solution**:
- Increase timeout in `appsettings.Test.json`
- Check if application is running
- Verify network connectivity

### "Port already in use"

**Solution**:
- Change `BaseUrl` port in `appsettings.Test.json`
- Stop other instances of the application

## Contributing

When adding new tests:

1. Follow the Page Object Model pattern
2. Use appropriate priority (P0-P3) and feature traits
3. Name tests with descriptive names following the convention
4. Use FluentAssertions for all assertions
5. Clean up resources (close pages, contexts)
6. Add tests to the test plan documentation

## Resources

- [Test Plan](../../testplan.md) - Comprehensive test plan document
- [Playwright for .NET Documentation](https://playwright.dev/dotnet/)
- [xUnit Documentation](https://xunit.net/)
- [FluentAssertions Documentation](https://fluentassertions.com/)

## Support

For issues or questions about E2E tests, please refer to the test plan or create an issue in the repository.
