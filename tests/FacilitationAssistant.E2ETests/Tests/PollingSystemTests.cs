using FacilitationAssistant.E2ETests.Fixtures;
using FacilitationAssistant.E2ETests.PageObjects;

namespace FacilitationAssistant.E2ETests.Tests;

/// <summary>
/// P0 (Critical) tests for Questions & Polling System
/// Tests question creation, triggering, attendee responses, and results
/// </summary>
[Collection("E2E")]
[Trait("Priority", "P0")]
[Trait("Feature", "PollingSystem")]
public class PollingSystemTests : IAsyncLifetime
{
    private readonly PlaywrightFixture _playwrightFixture;
    private readonly WebApplicationFixture _webAppFixture;
    private IBrowserContext? _context;
    private IPage? _page;

    public PollingSystemTests(PlaywrightFixture playwrightFixture, WebApplicationFixture webAppFixture)
    {
        _playwrightFixture = playwrightFixture;
        _webAppFixture = webAppFixture;
    }

    public async Task InitializeAsync()
    {
        _context = await _playwrightFixture.CreateContextAsync(recordVideo: true, recordTrace: true);
        _page = await _context.NewPageAsync();
    }

    public async Task DisposeAsync()
    {
        if (_page != null)
        {
            await _page.CloseAsync();
        }
        if (_context != null)
        {
            await _context.CloseAsync();
        }
    }

    [Fact]
    public async Task P0_020_CreateSingleChoiceQuestion_ShouldSaveSuccessfully()
    {
        // Arrange - Setup meeting
        var (facilitatorPage, _) = await CreateMeetingAndStartAsync();

        // Act - Create question
        var options = new[] { "Option A", "Option B", "Option C" };
        await facilitatorPage.CreateQuestionAsync("What is your preference?", "single-choice", options);
        await Task.Delay(500);

        // Assert - Question should be visible in facilitator's question list
        var questionExists = await _page!.IsVisibleAsync("[data-testid='question-item'], .question");
        questionExists.Should().BeTrue("question should be created and visible");
    }

    [Fact]
    public async Task P0_021_TriggerQuestion_ShouldAppearToAttendeesWithin2Seconds()
    {
        // Arrange - Setup meeting with facilitator and attendee
        var (facilitatorPage, attendeeLink) = await CreateMeetingAndStartAsync();
        
        // Setup attendee
        var attendeePage = await _context!.NewPageAsync();
        await attendeePage.GotoAsync(attendeeLink!);
        await attendeePage.WaitForLoadStateAsync(LoadState.NetworkIdle);
        var attendeePageObject = new AttendeePage(attendeePage, _playwrightFixture.Settings.BaseUrl);
        await attendeePageObject.WaitForMeetingStartAsync();

        // Act - Facilitator triggers question
        var startTime = DateTime.Now;
        var options = new[] { "Yes", "No" };
        await facilitatorPage.CreateQuestionAsync("Are you ready?", "single-choice", options);
        
        // Assert - Attendee should see question within 2 seconds
        await Task.Delay(2000); // Wait maximum 2 seconds
        var syncTime = DateTime.Now - startTime;
        
        var questionDisplayed = await attendeePageObject.IsQuestionDisplayedAsync();
        questionDisplayed.Should().BeTrue("attendee should see question");
        syncTime.TotalSeconds.Should().BeLessThanOrEqualTo(2, "question should appear within 2 seconds");

        await attendeePage.CloseAsync();
    }

    [Fact]
    public async Task P0_022_AttendeeSubmitsSingleChoiceAnswer_ShouldRecordResponse()
    {
        // Arrange - Setup meeting with question
        var (facilitatorPage, attendeeLink) = await CreateMeetingAndStartAsync();
        
        var attendeePage = await _context!.NewPageAsync();
        await attendeePage.GotoAsync(attendeeLink!);
        await attendeePage.WaitForLoadStateAsync(LoadState.NetworkIdle);
        var attendeePageObject = new AttendeePage(attendeePage, _playwrightFixture.Settings.BaseUrl);
        await attendeePageObject.WaitForMeetingStartAsync();

        // Trigger question
        var options = new[] { "Option A", "Option B" };
        await facilitatorPage.CreateQuestionAsync("Choose one:", "single-choice", options);
        await Task.Delay(2000);

        // Act - Attendee answers
        await attendeePageObject.AnswerSingleChoiceAsync(0); // Select first option
        await Task.Delay(1000);

        // Assert - Confirmation should be shown
        var confirmationVisible = await attendeePage.IsVisibleAsync(":has-text('submitted')");
        confirmationVisible.Should().BeTrue("confirmation message should appear after submission");
        
        // Question should be dismissed
        var questionStillVisible = await attendeePageObject.IsQuestionDisplayedAsync();
        questionStillVisible.Should().BeFalse("question should be dismissed after answering");

        await attendeePage.CloseAsync();
    }

    [Fact]
    public async Task P0_023_FacilitatorViewsRealTimeResults_ShouldShowResponseCount()
    {
        // Arrange - Setup meeting with multiple attendees
        var (facilitatorPage, attendeeLink) = await CreateMeetingAndStartAsync();
        
        // Create 2 attendees
        var attendee1 = await _context!.NewPageAsync();
        var attendee2 = await _context!.NewPageAsync();
        
        await attendee1.GotoAsync(attendeeLink!);
        await attendee2.GotoAsync(attendeeLink!);
        await Task.Delay(1000);

        var attendee1PageObject = new AttendeePage(attendee1, _playwrightFixture.Settings.BaseUrl);
        var attendee2PageObject = new AttendeePage(attendee2, _playwrightFixture.Settings.BaseUrl);
        
        await attendee1PageObject.WaitForMeetingStartAsync();
        await attendee2PageObject.WaitForMeetingStartAsync();

        // Trigger question
        var options = new[] { "Yes", "No" };
        await facilitatorPage.CreateQuestionAsync("Question for all:", "single-choice", options);
        await Task.Delay(2000);

        // Act - Both attendees answer
        await attendee1PageObject.AnswerSingleChoiceAsync(0); // Yes
        await attendee2PageObject.AnswerSingleChoiceAsync(1); // No
        await Task.Delay(2000); // Wait for results to update

        // Assert - Facilitator should see results
        var resultsVisible = await _page!.IsVisibleAsync("[data-testid='poll-results'], .results");
        resultsVisible.Should().BeTrue("facilitator should see poll results");
        
        // Check for response count
        var resultsText = await _page.Locator("[data-testid='poll-results'], .results").TextContentAsync();
        resultsText.Should().Contain("2", "should show 2 responses");

        await attendee1.CloseAsync();
        await attendee2.CloseAsync();
    }

    [Fact]
    public async Task P0_024_AttendeeSkaipsQuestion_ShouldRecordAsSkipped()
    {
        // Arrange
        var (facilitatorPage, attendeeLink) = await CreateMeetingAndStartAsync();
        
        var attendeePage = await _context!.NewPageAsync();
        await attendeePage.GotoAsync(attendeeLink!);
        await attendeePage.WaitForLoadStateAsync(LoadState.NetworkIdle);
        var attendeePageObject = new AttendeePage(attendeePage, _playwrightFixture.Settings.BaseUrl);
        await attendeePageObject.WaitForMeetingStartAsync();

        var options = new[] { "Option A", "Option B" };
        await facilitatorPage.CreateQuestionAsync("Optional question:", "single-choice", options);
        await Task.Delay(2000);

        // Act - Attendee skips
        await attendeePageObject.SkipQuestionAsync();
        await Task.Delay(1000);

        // Assert - Question should be dismissed
        var questionVisible = await attendeePageObject.IsQuestionDisplayedAsync();
        questionVisible.Should().BeFalse("question should be dismissed after skipping");

        await attendeePage.CloseAsync();
    }

    /// <summary>
    /// Helper method to create a meeting, add stage, and start it
    /// </summary>
    private async Task<(FacilitatorPage facilitatorPage, string? attendeeLink)> CreateMeetingAndStartAsync()
    {
        var homePage = new HomePage(_page!, _playwrightFixture.Settings.BaseUrl);
        await homePage.NavigateAsync();
        await homePage.ClickCreateMeetingAsync();
        var facilitatorLink = await homePage.GetFacilitatorLinkAsync();
        var attendeeLink = await homePage.GetAttendeeLinkAsync();

        await _page!.GotoAsync(facilitatorLink!);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        var facilitatorPage = new FacilitatorPage(_page, _playwrightFixture.Settings.BaseUrl);
        await facilitatorPage.AddAgendaStageAsync("Test Stage", 10);
        await facilitatorPage.StartMeetingAsync();

        return (facilitatorPage, attendeeLink);
    }
}
