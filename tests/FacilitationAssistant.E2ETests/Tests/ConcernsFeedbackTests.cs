using FacilitationAssistant.E2ETests.Fixtures;
using FacilitationAssistant.E2ETests.PageObjects;

namespace FacilitationAssistant.E2ETests.Tests;

/// <summary>
/// P1 (High) tests for Concerns & Feedback System
/// Tests raising concerns, voting, acknowledgment, and responses
/// </summary>
[Collection("E2E")]
[Trait("Priority", "P1")]
[Trait("Feature", "ConcernsFeedback")]
public class ConcernsFeedbackTests : IAsyncLifetime
{
    private readonly PlaywrightFixture _playwrightFixture;
    private readonly WebApplicationFixture _webAppFixture;
    private IBrowserContext? _context;
    private IPage? _page;

    public ConcernsFeedbackTests(PlaywrightFixture playwrightFixture, WebApplicationFixture webAppFixture)
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
    public async Task P1_040_AttendeeRaisesPredefinedConcern_ShouldAppearInPanel()
    {
        // Arrange
        var (facilitatorPage, attendeeLink) = await CreateMeetingAndStartAsync();
        
        var attendeePage = await _context!.NewPageAsync();
        await attendeePage.GotoAsync(attendeeLink!);
        await attendeePage.WaitForLoadStateAsync(LoadState.NetworkIdle);
        var attendeePageObject = new AttendeePage(attendeePage, _playwrightFixture.Settings.BaseUrl);
        await attendeePageObject.WaitForMeetingStartAsync();

        // Act - Raise concern
        await attendeePageObject.RaisePredefinedConcernAsync("meeting-overrunning");
        await Task.Delay(2000);

        // Assert - Concern should be visible
        var concernCount = await attendeePageObject.GetConcernCountAsync();
        concernCount.Should().BeGreaterThan(0, "concern should appear in panel");

        await attendeePage.CloseAsync();
    }

    [Fact]
    public async Task P1_041_AttendeeRaisesCustomConcern_ShouldDisplayText()
    {
        // Arrange
        var (facilitatorPage, attendeeLink) = await CreateMeetingAndStartAsync();
        
        var attendeePage = await _context!.NewPageAsync();
        await attendeePage.GotoAsync(attendeeLink!);
        await attendeePage.WaitForLoadStateAsync(LoadState.NetworkIdle);
        var attendeePageObject = new AttendeePage(attendeePage, _playwrightFixture.Settings.BaseUrl);
        await attendeePageObject.WaitForMeetingStartAsync();

        // Act - Raise custom concern
        var customText = "Audio quality is poor";
        await attendeePageObject.RaiseCustomConcernAsync(customText);
        await Task.Delay(2000);

        // Assert - Custom concern text should be visible
        var concernText = await attendeePage.Locator("[data-testid='concern-item'], .concern").TextContentAsync();
        concernText.Should().Contain(customText, "custom concern text should be displayed");

        await attendeePage.CloseAsync();
    }

    [Fact]
    public async Task P1_042_AttendeeVotesOnConcern_ShouldUpdateCount()
    {
        // Arrange - Raise a concern first
        var (facilitatorPage, attendeeLink) = await CreateMeetingAndStartAsync();
        
        var attendee1Page = await _context!.NewPageAsync();
        await attendee1Page.GotoAsync(attendeeLink!);
        await attendee1Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        var attendee1PageObject = new AttendeePage(attendee1Page, _playwrightFixture.Settings.BaseUrl);
        await attendee1PageObject.WaitForMeetingStartAsync();
        
        await attendee1PageObject.RaisePredefinedConcernAsync("topic-unclear");
        await Task.Delay(2000);

        // Setup second attendee
        var attendee2Page = await _context!.NewPageAsync();
        await attendee2Page.GotoAsync(attendeeLink!);
        await attendee2Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        var attendee2PageObject = new AttendeePage(attendee2Page, _playwrightFixture.Settings.BaseUrl);
        await attendee2PageObject.WaitForMeetingStartAsync();

        // Act - Second attendee votes on concern
        await attendee2PageObject.VoteOnConcernAsync(0, "like");
        await Task.Delay(2000);

        // Assert - Vote count should be visible
        var concernText = await attendee2Page.Locator("[data-testid='concern-item']:first-of-type").TextContentAsync();
        concernText.Should().Contain("1", "vote count should be displayed");

        await attendee1Page.CloseAsync();
        await attendee2Page.CloseAsync();
    }

    [Fact]
    public async Task P1_043_FacilitatorAcknowledgesConcern_ShouldUpdateStatus()
    {
        // Arrange
        var (facilitatorPage, attendeeLink) = await CreateMeetingAndStartAsync();
        
        var attendeePage = await _context!.NewPageAsync();
        await attendeePage.GotoAsync(attendeeLink!);
        await attendeePage.WaitForLoadStateAsync(LoadState.NetworkIdle);
        var attendeePageObject = new AttendeePage(attendeePage, _playwrightFixture.Settings.BaseUrl);
        await attendeePageObject.WaitForMeetingStartAsync();
        
        await attendeePageObject.RaisePredefinedConcernAsync("need-break");
        await Task.Delay(2000);

        // Act - Facilitator opens panel and acknowledges
        await facilitatorPage.OpenConcernsPanelAsync();
        await facilitatorPage.AcknowledgeConcernAsync(0);
        await Task.Delay(2000);

        // Assert - Concern should show acknowledged status
        var concernText = await _page!.Locator("[data-testid='concern-item']:first-of-type").TextContentAsync();
        concernText.Should().Contain("Acknowledged", "concern should show acknowledged status");

        await attendeePage.CloseAsync();
    }

    [Fact]
    public async Task P1_044_AttendeeWithdrawsConcern_ShouldMarkAsWithdrawn()
    {
        // Arrange
        var (facilitatorPage, attendeeLink) = await CreateMeetingAndStartAsync();
        
        var attendeePage = await _context!.NewPageAsync();
        await attendeePage.GotoAsync(attendeeLink!);
        await attendeePage.WaitForLoadStateAsync(LoadState.NetworkIdle);
        var attendeePageObject = new AttendeePage(attendeePage, _playwrightFixture.Settings.BaseUrl);
        await attendeePageObject.WaitForMeetingStartAsync();
        
        await attendeePageObject.RaiseCustomConcernAsync("Test concern to withdraw");
        await Task.Delay(2000);

        // Act - Withdraw the concern
        await attendeePageObject.WithdrawConcernAsync(0);
        await Task.Delay(2000);

        // Assert - Concern should be marked withdrawn
        var concernText = await attendeePage.Locator("[data-testid='concern-item']:first-of-type").TextContentAsync();
        concernText.Should().Contain("Withdrawn", "concern should show withdrawn status");

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
        await facilitatorPage.AddAgendaStageAsync("Test Stage", 15);
        await facilitatorPage.StartMeetingAsync();

        return (facilitatorPage, attendeeLink);
    }
}
