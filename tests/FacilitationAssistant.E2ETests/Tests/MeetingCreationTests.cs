using FacilitationAssistant.E2ETests.Fixtures;
using FacilitationAssistant.E2ETests.PageObjects;

namespace FacilitationAssistant.E2ETests.Tests;

/// <summary>
/// P0 (Critical) tests for Meeting Creation and Link Management feature
/// Tests core functionality: creating meetings, generating links, and access control
/// </summary>
[Collection("E2E")]
[Trait("Priority", "P0")]
[Trait("Feature", "MeetingCreation")]
public class MeetingCreationTests : IAsyncLifetime
{
    private readonly PlaywrightFixture _playwrightFixture;
    private readonly WebApplicationFixture _webAppFixture;
    private IBrowserContext? _context;
    private IPage? _page;

    public MeetingCreationTests(PlaywrightFixture playwrightFixture, WebApplicationFixture webAppFixture)
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
    public async Task P0_001_CreateMeeting_ShouldGenerateUniqueFacilitatorAndAttendeeLinks()
    {
        // Arrange
        var homePage = new HomePage(_page!, _playwrightFixture.Settings.BaseUrl);
        await homePage.NavigateAsync();

        // Act
        await homePage.ClickCreateMeetingAsync();
        var facilitatorLink = await homePage.GetFacilitatorLinkAsync();
        var attendeeLink = await homePage.GetAttendeeLinkAsync();

        // Assert
        facilitatorLink.Should().NotBeNullOrEmpty("facilitator link should be generated");
        attendeeLink.Should().NotBeNullOrEmpty("attendee link should be generated");
        facilitatorLink.Should().NotBe(attendeeLink, "facilitator and attendee links should be different");
        
        // Verify both links are displayed
        var linksDisplayed = await homePage.AreLinksDisplayedAsync();
        linksDisplayed.Should().BeTrue("both links should be visible on the page");
        
        // Verify link format (unique, non-sequential, ≥16 chars)
        homePage.IsValidLink(facilitatorLink).Should().BeTrue("facilitator link should have valid format");
        homePage.IsValidLink(attendeeLink).Should().BeTrue("attendee link should have valid format");
    }

    [Fact]
    public async Task P0_002_FacilitatorLink_ShouldProvideFullAccessControl()
    {
        // Arrange
        var homePage = new HomePage(_page!, _playwrightFixture.Settings.BaseUrl);
        await homePage.NavigateAsync();
        await homePage.ClickCreateMeetingAsync();
        var facilitatorLink = await homePage.GetFacilitatorLinkAsync();

        // Act
        await _page!.GotoAsync(facilitatorLink!);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Assert
        var facilitatorPage = new FacilitatorPage(_page, _playwrightFixture.Settings.BaseUrl);
        
        // Verify facilitator controls are visible
        var addStageVisible = await _page.IsVisibleAsync("[data-testid='add-stage'], button:has-text('Add Stage')");
        addStageVisible.Should().BeTrue("'Add Stage' button should be visible for facilitator");
        
        var startMeetingVisible = await _page.IsVisibleAsync("[data-testid='start-meeting'], button:has-text('Start Meeting')");
        startMeetingVisible.Should().BeTrue("'Start Meeting' button should be visible for facilitator");
    }

    [Fact]
    public async Task P0_003_AttendeeLink_BeforeMeetingStart_ShouldShowWaitingMessage()
    {
        // Arrange
        var homePage = new HomePage(_page!, _playwrightFixture.Settings.BaseUrl);
        await homePage.NavigateAsync();
        await homePage.ClickCreateMeetingAsync();
        var attendeeLink = await homePage.GetAttendeeLinkAsync();

        // Act
        await _page!.GotoAsync(attendeeLink!);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Assert
        var attendeePage = new AttendeePage(_page, _playwrightFixture.Settings.BaseUrl);
        var waitingMessageDisplayed = await attendeePage.IsWaitingMessageDisplayedAsync();
        waitingMessageDisplayed.Should().BeTrue("waiting message should be displayed before meeting starts");
        
        // Verify facilitator controls are NOT visible
        var addStageVisible = await _page.IsVisibleAsync("[data-testid='add-stage'], button:has-text('Add Stage')");
        addStageVisible.Should().BeFalse("attendee should not see facilitator controls");
    }

    [Fact]
    public async Task P0_004_AttendeeLink_AfterMeetingStart_ShouldShowFullAgenda()
    {
        // Arrange - Create meeting and start it
        var homePage = new HomePage(_page!, _playwrightFixture.Settings.BaseUrl);
        await homePage.NavigateAsync();
        await homePage.ClickCreateMeetingAsync();
        var facilitatorLink = await homePage.GetFacilitatorLinkAsync();
        var attendeeLink = await homePage.GetAttendeeLinkAsync();

        // Facilitator adds stage and starts meeting
        await _page!.GotoAsync(facilitatorLink!);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        var facilitatorPage = new FacilitatorPage(_page, _playwrightFixture.Settings.BaseUrl);
        await facilitatorPage.AddAgendaStageAsync("Introduction", 5);
        await facilitatorPage.StartMeetingAsync();

        // Act - Attendee accesses meeting
        var attendeePage = await _context!.NewPageAsync();
        await attendeePage.GotoAsync(attendeeLink!);
        await attendeePage.WaitForLoadStateAsync(LoadState.NetworkIdle);
        var attendeePageObject = new AttendeePage(attendeePage, _playwrightFixture.Settings.BaseUrl);

        // Assert
        var meetingStarted = await attendeePageObject.WaitForMeetingStartAsync(5000);
        meetingStarted.Should().BeTrue("attendee should see meeting started");
        
        // Verify no facilitator controls are visible
        var facilitatorControls = await attendeePage.IsVisibleAsync("[data-testid='start-meeting'], button:has-text('Start Meeting')");
        facilitatorControls.Should().BeFalse("attendee should not see facilitator controls");

        await attendeePage.CloseAsync();
    }

    [Fact]
    public async Task P0_005_LinkPersistence_ShouldMaintainStateAcrossSessions()
    {
        // Arrange - Create meeting and add stage
        var homePage = new HomePage(_page!, _playwrightFixture.Settings.BaseUrl);
        await homePage.NavigateAsync();
        await homePage.ClickCreateMeetingAsync();
        var facilitatorLink = await homePage.GetFacilitatorLinkAsync();

        await _page!.GotoAsync(facilitatorLink!);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        var facilitatorPage = new FacilitatorPage(_page, _playwrightFixture.Settings.BaseUrl);
        await facilitatorPage.AddAgendaStageAsync("Test Stage", 10);

        // Act - Close browser and reopen
        await _page.CloseAsync();
        await _context!.CloseAsync();

        // Create new context and page (simulating browser close/reopen)
        _context = await _playwrightFixture.CreateContextAsync();
        _page = await _context.NewPageAsync();
        await _page.GotoAsync(facilitatorLink!);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Assert - Verify meeting state persisted
        facilitatorPage = new FacilitatorPage(_page, _playwrightFixture.Settings.BaseUrl);
        var stageCount = await facilitatorPage.GetStageCountAsync();
        stageCount.Should().BeGreaterThan(0, "meeting state should persist across sessions");
    }

    [Fact]
    public async Task P0_006_AttendeeLink_CannotAccessFacilitatorFunctions()
    {
        // Arrange
        var homePage = new HomePage(_page!, _playwrightFixture.Settings.BaseUrl);
        await homePage.NavigateAsync();
        await homePage.ClickCreateMeetingAsync();
        var attendeeLink = await homePage.GetAttendeeLinkAsync();

        // Act
        await _page!.GotoAsync(attendeeLink!);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Assert - Verify facilitator-only controls are not present
        var addStageButton = await _page.QuerySelectorAsync("[data-testid='add-stage'], button:has-text('Add Stage')");
        addStageButton.Should().BeNull("attendee should not see 'Add Stage' button");
        
        var startMeetingButton = await _page.QuerySelectorAsync("[data-testid='start-meeting'], button:has-text('Start Meeting')");
        startMeetingButton.Should().BeNull("attendee should not see 'Start Meeting' button");
        
        var endMeetingButton = await _page.QuerySelectorAsync("[data-testid='end-meeting'], button:has-text('End Meeting')");
        endMeetingButton.Should().BeNull("attendee should not see 'End Meeting' button");
    }

    [Fact]
    public async Task P0_007_ParallelMeetingCreation_ShouldGenerateUniqueIDs()
    {
        // Arrange
        var tasks = new List<Task<(string? facilitatorLink, string? attendeeLink)>>();

        // Act - Create 5 meetings in parallel
        for (int i = 0; i < 5; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                var context = await _playwrightFixture.CreateContextAsync();
                var page = await context.NewPageAsync();
                var homePage = new HomePage(page, _playwrightFixture.Settings.BaseUrl);
                await homePage.NavigateAsync();
                await homePage.ClickCreateMeetingAsync();
                var facilitatorLink = await homePage.GetFacilitatorLinkAsync();
                var attendeeLink = await homePage.GetAttendeeLinkAsync();
                await page.CloseAsync();
                await context.CloseAsync();
                return (facilitatorLink, attendeeLink);
            }));
        }

        var results = await Task.WhenAll(tasks);

        // Assert - All links should be unique
        var allFacilitatorLinks = results.Select(r => r.facilitatorLink).ToList();
        var allAttendeeLinks = results.Select(r => r.attendeeLink).ToList();

        allFacilitatorLinks.Should().OnlyHaveUniqueItems("all facilitator links should be unique");
        allAttendeeLinks.Should().OnlyHaveUniqueItems("all attendee links should be unique");
    }
}

/// <summary>
/// xUnit collection definition for E2E tests
/// Ensures fixtures are shared across test classes
/// </summary>
[CollectionDefinition("E2E")]
public class E2ETestCollection : ICollectionFixture<PlaywrightFixture>, ICollectionFixture<WebApplicationFixture>
{
    // This class is never instantiated, it's just a marker for the collection
}
