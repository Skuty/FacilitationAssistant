using FacilitationAssistant.E2ETests.Utilities;

namespace FacilitationAssistant.E2ETests.PageObjects;

/// <summary>
/// Page Object for the home page (meeting creation)
/// </summary>
public class HomePage : BasePage
{
    // Selectors
    private const string CreateMeetingButton = "button:has-text('Create')";
    private const string FacilitatorLinkInput = "[data-testid='facilitator-link'], input[readonly]:nth-of-type(1)";
    private const string AttendeeLinkInput = "[data-testid='attendee-link'], input[readonly]:nth-of-type(2)";
    private const string FacilitatorLinkCopy = "[data-testid='copy-facilitator-link']";
    private const string AttendeeLinkCopy = "[data-testid='copy-attendee-link']";

    public HomePage(IPage page, string baseUrl) : base(page, baseUrl)
    {
    }

    public override async Task NavigateAsync(string path = "")
    {
        await base.NavigateAsync("/");
    }

    /// <summary>
    /// Clicks the "Create Meeting" button
    /// </summary>
    public async Task ClickCreateMeetingAsync()
    {
        await Page.ClickAsync(CreateMeetingButton);
        await Task.Delay(1000); // Wait for navigation/processing
        await WaitForLoadAsync();
    }

    /// <summary>
    /// Gets the facilitator link after meeting creation
    /// </summary>
    public async Task<string?> GetFacilitatorLinkAsync()
    {
        try
        {
            await WaitForElementAsync(FacilitatorLinkInput, 5000);
            var value = await Page.InputValueAsync(FacilitatorLinkInput);
            return value;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Gets the attendee link after meeting creation
    /// </summary>
    public async Task<string?> GetAttendeeLinkAsync()
    {
        try
        {
            await WaitForElementAsync(AttendeeLinkInput, 5000);
            var value = await Page.InputValueAsync(AttendeeLinkInput);
            return value;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Copies the facilitator link to clipboard
    /// </summary>
    public async Task CopyFacilitatorLinkAsync()
    {
        await Page.ClickAsync(FacilitatorLinkCopy);
        await Task.Delay(500);
    }

    /// <summary>
    /// Copies the attendee link to clipboard
    /// </summary>
    public async Task CopyAttendeeLinkAsync()
    {
        await Page.ClickAsync(AttendeeLinkCopy);
        await Task.Delay(500);
    }

    /// <summary>
    /// Checks if both links are displayed
    /// </summary>
    public async Task<bool> AreLinksDisplayedAsync()
    {
        var facilitatorVisible = await IsVisibleAsync(FacilitatorLinkInput);
        var attendeeVisible = await IsVisibleAsync(AttendeeLinkInput);
        return facilitatorVisible && attendeeVisible;
    }

    /// <summary>
    /// Validates link format and length
    /// </summary>
    public bool IsValidLink(string? link)
    {
        if (string.IsNullOrEmpty(link))
            return false;

        // Extract the ID part from URL
        var uri = new Uri(link);
        var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var id = segments.Length > 1 ? segments.Last() : "";

        // Check if ID is at least 16 characters and appears random
        return id.Length >= 16 && !IsSequential(id);
    }

    private bool IsSequential(string value)
    {
        // Basic check for sequential patterns (e.g., "123456", "abcdef")
        if (value.Length < 3) return false;
        
        for (int i = 0; i < value.Length - 2; i++)
        {
            if (char.IsDigit(value[i]) && 
                value[i] + 1 == value[i + 1] && 
                value[i] + 2 == value[i + 2])
            {
                return true;
            }
        }
        return false;
    }
}
