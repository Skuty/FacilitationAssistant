namespace FacilitationAssistant.E2ETests.Utilities;

/// <summary>
/// Helper methods for common Playwright operations
/// </summary>
public static class BrowserHelpers
{
    /// <summary>
    /// Waits for an element to be visible and returns it
    /// </summary>
    public static async Task<IElementHandle?> WaitForElementAsync(
        this IPage page, 
        string selector, 
        int timeoutMs = 10000)
    {
        try
        {
            await page.WaitForSelectorAsync(selector, new PageWaitForSelectorOptions 
            { 
                State = WaitForSelectorState.Visible,
                Timeout = timeoutMs
            });
            return await page.QuerySelectorAsync(selector);
        }
        catch (TimeoutException)
        {
            return null;
        }
    }

    /// <summary>
    /// Clicks an element by selector with retry logic
    /// </summary>
    public static async Task ClickAsync(this IPage page, string selector, int retries = 3)
    {
        for (int i = 0; i < retries; i++)
        {
            try
            {
                await page.ClickAsync(selector, new PageClickOptions { Timeout = 5000 });
                return;
            }
            catch (TimeoutException) when (i < retries - 1)
            {
                await Task.Delay(500);
            }
        }
    }

    /// <summary>
    /// Fills input field with text and waits for updates
    /// </summary>
    public static async Task FillAndWaitAsync(
        this IPage page, 
        string selector, 
        string value, 
        int delayMs = 100)
    {
        await page.FillAsync(selector, value);
        await Task.Delay(delayMs); // Allow for debounced updates
    }

    /// <summary>
    /// Takes a screenshot and saves it to test results
    /// </summary>
    public static async Task SaveScreenshotAsync(this IPage page, string name)
    {
        var dir = Path.Combine(Directory.GetCurrentDirectory(), "test-results", "screenshots");
        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, $"{name}_{DateTime.Now:yyyyMMdd_HHmmss}.png");
        await page.ScreenshotAsync(new PageScreenshotOptions { Path = path, FullPage = true });
    }

    /// <summary>
    /// Waits for SignalR connection to be established (Blazor Server)
    /// </summary>
    public static async Task WaitForBlazorAsync(this IPage page, int timeoutMs = 10000)
    {
        try
        {
            // Wait for Blazor to be initialized (look for blazor script loaded)
            await page.WaitForFunctionAsync("() => window.Blazor !== undefined", new PageWaitForFunctionOptions 
            { 
                Timeout = timeoutMs 
            });
            
            // Give SignalR a moment to connect
            await Task.Delay(500);
        }
        catch (TimeoutException)
        {
            // Blazor may already be loaded, continue
        }
    }

    /// <summary>
    /// Gets text content of an element by selector
    /// </summary>
    public static async Task<string?> GetTextAsync(this IPage page, string selector)
    {
        var element = await page.QuerySelectorAsync(selector);
        return element != null ? await element.TextContentAsync() : null;
    }

    /// <summary>
    /// Checks if an element exists on the page
    /// </summary>
    public static async Task<bool> ElementExistsAsync(this IPage page, string selector)
    {
        var element = await page.QuerySelectorAsync(selector);
        return element != null;
    }

    /// <summary>
    /// Waits for a condition to be true with timeout
    /// </summary>
    public static async Task<bool> WaitForConditionAsync(
        Func<Task<bool>> condition, 
        int timeoutMs = 5000, 
        int pollIntervalMs = 100)
    {
        var endTime = DateTime.Now.AddMilliseconds(timeoutMs);
        
        while (DateTime.Now < endTime)
        {
            if (await condition())
                return true;
            
            await Task.Delay(pollIntervalMs);
        }
        
        return false;
    }
}
