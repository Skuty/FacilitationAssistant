using Microsoft.Extensions.Configuration;

namespace FacilitationAssistant.E2ETests.Fixtures;

/// <summary>
/// Manages Playwright browser lifecycle for E2E tests
/// Implements IAsyncLifetime for xUnit async initialization/disposal
/// </summary>
public class PlaywrightFixture : IAsyncLifetime
{
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    public TestSettings Settings { get; private set; }

    public PlaywrightFixture()
    {
        // Load test settings from appsettings.Test.json
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.Test.json")
            .Build();

        Settings = new TestSettings();
        config.GetSection("TestSettings").Bind(Settings);
    }

    public IBrowser Browser => _browser ?? throw new InvalidOperationException("Browser not initialized");

    public async Task InitializeAsync()
    {
        // Install Playwright browsers if not already installed
        var exitCode = Microsoft.Playwright.Program.Main(new[] { "install" });
        if (exitCode != 0)
        {
            throw new Exception($"Playwright install failed with exit code {exitCode}");
        }

        // Create Playwright instance
        _playwright = await Playwright.CreateAsync();

        // Launch browser based on configuration
        _browser = Settings.BrowserType.ToLowerInvariant() switch
        {
            "firefox" => await _playwright.Firefox.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = Settings.Headless,
                SlowMo = Settings.SlowMo
            }),
            "webkit" => await _playwright.Webkit.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = Settings.Headless,
                SlowMo = Settings.SlowMo
            }),
            _ => await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = Settings.Headless,
                SlowMo = Settings.SlowMo
            })
        };
    }

    public async Task DisposeAsync()
    {
        if (_browser != null)
        {
            await _browser.CloseAsync();
            await _browser.DisposeAsync();
        }
        _playwright?.Dispose();
    }

    /// <summary>
    /// Creates a new browser context with optional video/trace recording
    /// </summary>
    public async Task<IBrowserContext> CreateContextAsync(bool recordVideo = false, bool recordTrace = false)
    {
        var options = new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Width = 1920, Height = 1080 },
            IgnoreHTTPSErrors = true
        };

        if (recordVideo && Settings.VideoOnFailure)
        {
            options.RecordVideoDir = Path.Combine(Directory.GetCurrentDirectory(), "test-results", "videos");
        }

        var context = await Browser.NewContextAsync(options);

        if (recordTrace && Settings.TraceOnFailure)
        {
            await context.Tracing.StartAsync(new TracingStartOptions
            {
                Screenshots = true,
                Snapshots = true,
                Sources = true
            });
        }

        context.SetDefaultTimeout(Settings.Timeout);
        return context;
    }
}
