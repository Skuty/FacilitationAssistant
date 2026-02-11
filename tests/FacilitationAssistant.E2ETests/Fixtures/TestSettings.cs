namespace FacilitationAssistant.E2ETests.Fixtures;

/// <summary>
/// Configuration settings for E2E tests loaded from appsettings.Test.json
/// </summary>
public class TestSettings
{
    public string BaseUrl { get; set; } = "http://localhost:5000";
    public string BrowserType { get; set; } = "chromium";
    public bool Headless { get; set; } = true;
    public int SlowMo { get; set; } = 0;
    public int Timeout { get; set; } = 30000;
    public bool VideoOnFailure { get; set; } = true;
    public bool ScreenshotOnFailure { get; set; } = true;
    public bool TraceOnFailure { get; set; } = true;
}
