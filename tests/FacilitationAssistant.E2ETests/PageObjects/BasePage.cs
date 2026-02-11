using FacilitationAssistant.E2ETests.Utilities;

namespace FacilitationAssistant.E2ETests.PageObjects;

/// <summary>
/// Base class for all page objects providing common functionality
/// </summary>
public abstract class BasePage
{
    protected IPage Page;
    protected string BaseUrl;

    protected BasePage(IPage page, string baseUrl)
    {
        Page = page;
        BaseUrl = baseUrl;
    }

    /// <summary>
    /// Navigates to the page URL
    /// </summary>
    public virtual async Task NavigateAsync(string path = "")
    {
        await Page.GotoAsync($"{BaseUrl}{path}", new PageGotoOptions 
        { 
            WaitUntil = WaitUntilState.NetworkIdle 
        });
        await Page.WaitForBlazorAsync();
    }

    /// <summary>
    /// Waits for the page to be fully loaded
    /// </summary>
    public async Task WaitForLoadAsync()
    {
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Page.WaitForBlazorAsync();
    }

    /// <summary>
    /// Takes a screenshot for debugging
    /// </summary>
    public async Task TakeScreenshotAsync(string name)
    {
        await Page.SaveScreenshotAsync(name);
    }

    /// <summary>
    /// Gets the page title
    /// </summary>
    public async Task<string> GetTitleAsync()
    {
        return await Page.TitleAsync();
    }

    /// <summary>
    /// Checks if an element is visible
    /// </summary>
    protected async Task<bool> IsVisibleAsync(string selector)
    {
        return await Page.IsVisibleAsync(selector);
    }

    /// <summary>
    /// Waits for an element to appear
    /// </summary>
    protected async Task WaitForElementAsync(string selector, int timeoutMs = 10000)
    {
        await Page.WaitForSelectorAsync(selector, new PageWaitForSelectorOptions 
        { 
            State = WaitForSelectorState.Visible,
            Timeout = timeoutMs 
        });
    }

    /// <summary>
    /// Gets text content of an element
    /// </summary>
    protected async Task<string?> GetTextAsync(string selector)
    {
        return await Page.GetTextAsync(selector);
    }
}
