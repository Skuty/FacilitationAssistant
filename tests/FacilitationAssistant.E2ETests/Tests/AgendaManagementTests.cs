using FacilitationAssistant.E2ETests.Fixtures;
using FacilitationAssistant.E2ETests.PageObjects;

namespace FacilitationAssistant.E2ETests.Tests;

/// <summary>
/// P0 (Critical) tests for Agenda Management and Real-Time Timing feature
/// Tests stage management, timers, and real-time synchronization
/// </summary>
[Collection("E2E")]
[Trait("Priority", "P0")]
[Trait("Feature", "AgendaManagement")]
public class AgendaManagementTests : IAsyncLifetime
{
    private readonly PlaywrightFixture _playwrightFixture;
    private readonly WebApplicationFixture _webAppFixture;
    private IBrowserContext? _context;
    private IPage? _page;

    public AgendaManagementTests(PlaywrightFixture playwrightFixture, WebApplicationFixture webAppFixture)
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
    public async Task P0_010_AddAgendaStages_ShouldDisplayInCorrectOrder()
    {
        // Arrange
        var homePage = new HomePage(_page!, _playwrightFixture.Settings.BaseUrl);
        await homePage.NavigateAsync();
        await homePage.ClickCreateMeetingAsync();
        var facilitatorLink = await homePage.GetFacilitatorLinkAsync();

        await _page!.GotoAsync(facilitatorLink!);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        var facilitatorPage = new FacilitatorPage(_page, _playwrightFixture.Settings.BaseUrl);

        // Act - Add multiple stages
        await facilitatorPage.AddAgendaStageAsync("Introduction", 5);
        await facilitatorPage.AddAgendaStageAsync("Discussion", 15);
        await facilitatorPage.AddAgendaStageAsync("Conclusion", 10);

        // Assert
        var stageCount = await facilitatorPage.GetStageCountAsync();
        stageCount.Should().Be(3, "three stages should be added");
    }

    [Fact]
    public async Task P0_011_StartStageTimer_ShouldActivateStageAndStartTimer()
    {
        // Arrange - Create meeting, add stage, start meeting
        var homePage = new HomePage(_page!, _playwrightFixture.Settings.BaseUrl);
        await homePage.NavigateAsync();
        await homePage.ClickCreateMeetingAsync();
        var facilitatorLink = await homePage.GetFacilitatorLinkAsync();

        await _page!.GotoAsync(facilitatorLink!);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        var facilitatorPage = new FacilitatorPage(_page, _playwrightFixture.Settings.BaseUrl);
        await facilitatorPage.AddAgendaStageAsync("Test Stage", 5);
        await facilitatorPage.StartMeetingAsync();

        // Act - Start the first stage
        await facilitatorPage.StartStageAsync(0);
        await Task.Delay(1000); // Wait for timer to start

        // Assert
        var isActive = await facilitatorPage.IsStageActiveAsync(0);
        isActive.Should().BeTrue("stage should be active after starting");
        
        // Verify timer is running (check if timer element exists and is visible)
        var timerVisible = await _page.IsVisibleAsync("[data-testid='timer'], .timer");
        timerVisible.Should().BeTrue("timer should be visible when stage is active");
    }

    [Fact]
    public async Task P0_012_EndStage_ShouldStopTimerAndMarkCompleted()
    {
        // Arrange - Start a stage
        var homePage = new HomePage(_page!, _playwrightFixture.Settings.BaseUrl);
        await homePage.NavigateAsync();
        await homePage.ClickCreateMeetingAsync();
        var facilitatorLink = await homePage.GetFacilitatorLinkAsync();

        await _page!.GotoAsync(facilitatorLink!);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        var facilitatorPage = new FacilitatorPage(_page, _playwrightFixture.Settings.BaseUrl);
        await facilitatorPage.AddAgendaStageAsync("Test Stage", 5);
        await facilitatorPage.StartMeetingAsync();
        await facilitatorPage.StartStageAsync(0);
        await Task.Delay(2000); // Let stage run for 2 seconds

        // Act - End the stage
        await facilitatorPage.EndCurrentStageAsync();
        await Task.Delay(1000);

        // Assert - Stage should be marked as completed
        var isActive = await facilitatorPage.IsStageActiveAsync(0);
        isActive.Should().BeFalse("stage should not be active after ending");
        
        // Check for completed status
        var completedIndicator = await _page.QuerySelectorAsync("[data-status='completed'], .completed");
        completedIndicator.Should().NotBeNull("stage should be marked as completed");
    }

    [Fact]
    public async Task P0_013_RealTimeStageSyncToAttendees_ShouldUpdateWithin2Seconds()
    {
        // Arrange - Create meeting with facilitator and attendee
        var homePage = new HomePage(_page!, _playwrightFixture.Settings.BaseUrl);
        await homePage.NavigateAsync();
        await homePage.ClickCreateMeetingAsync();
        var facilitatorLink = await homePage.GetFacilitatorLinkAsync();
        var attendeeLink = await homePage.GetAttendeeLinkAsync();

        // Facilitator setup
        await _page!.GotoAsync(facilitatorLink!);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        var facilitatorPage = new FacilitatorPage(_page, _playwrightFixture.Settings.BaseUrl);
        await facilitatorPage.AddAgendaStageAsync("Stage 1", 5);
        await facilitatorPage.AddAgendaStageAsync("Stage 2", 10);
        await facilitatorPage.StartMeetingAsync();

        // Attendee setup
        var attendeePage = await _context!.NewPageAsync();
        await attendeePage.GotoAsync(attendeeLink!);
        await attendeePage.WaitForLoadStateAsync(LoadState.NetworkIdle);
        var attendeePageObject = new AttendeePage(attendeePage, _playwrightFixture.Settings.BaseUrl);
        await attendeePageObject.WaitForMeetingStartAsync();

        // Act - Facilitator starts Stage 1
        var startTime = DateTime.Now;
        await facilitatorPage.StartStageAsync(0);

        // Assert - Attendee sees stage update within 2 seconds
        var synced = await attendeePageObject.WaitForStageSyncAsync("Stage 1", 2000);
        var syncTime = DateTime.Now - startTime;
        
        synced.Should().BeTrue("attendee should see stage transition");
        syncTime.TotalSeconds.Should().BeLessThanOrEqualTo(2, "sync should happen within 2 seconds");

        await attendeePage.CloseAsync();
    }

    [Fact]
    public async Task P0_014_OverallMeetingTimer_ShouldDisplayElapsedTime()
    {
        // Arrange - Start meeting
        var homePage = new HomePage(_page!, _playwrightFixture.Settings.BaseUrl);
        await homePage.NavigateAsync();
        await homePage.ClickCreateMeetingAsync();
        var facilitatorLink = await homePage.GetFacilitatorLinkAsync();

        await _page!.GotoAsync(facilitatorLink!);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        var facilitatorPage = new FacilitatorPage(_page, _playwrightFixture.Settings.BaseUrl);
        await facilitatorPage.AddAgendaStageAsync("Test Stage", 5);
        await facilitatorPage.StartMeetingAsync();
        await facilitatorPage.StartStageAsync(0);

        // Act - Wait for a few seconds
        await Task.Delay(3000);

        // Assert - Overall timer should be visible and updating
        var timerText = await _page.Locator("[data-testid='total-timer'], .total-timer").TextContentAsync();
        timerText.Should().NotBeNullOrEmpty("overall timer should display elapsed time");
        
        // Timer should show some elapsed time (at least 0m or more)
        timerText.Should().Contain("m", "timer should show minutes format");
    }

    [Fact]
    public async Task P0_015_StageOverrun_ShouldShowWarningIndicator()
    {
        // Arrange - Create short stage
        var homePage = new HomePage(_page!, _playwrightFixture.Settings.BaseUrl);
        await homePage.NavigateAsync();
        await homePage.ClickCreateMeetingAsync();
        var facilitatorLink = await homePage.GetFacilitatorLinkAsync();

        await _page!.GotoAsync(facilitatorLink!);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        var facilitatorPage = new FacilitatorPage(_page, _playwrightFixture.Settings.BaseUrl);
        
        // Create a stage with 1 minute duration
        await facilitatorPage.AddAgendaStageAsync("Quick Stage", 1);
        await facilitatorPage.StartMeetingAsync();
        await facilitatorPage.StartStageAsync(0);

        // Act - Wait for stage to overrun (65 seconds)
        await Task.Delay(65000);

        // Assert - Overrun indicator should be visible
        var overrunText = await _page.Locator("[data-testid='stage-0'], .stage").TextContentAsync();
        overrunText.Should().Contain("overrun", "stage has exceeded planned duration");
    }

    [Fact]
    public async Task P0_016_SkipStage_ShouldLeaveStageAsNotStarted()
    {
        // Arrange - Create multiple stages
        var homePage = new HomePage(_page!, _playwrightFixture.Settings.BaseUrl);
        await homePage.NavigateAsync();
        await homePage.ClickCreateMeetingAsync();
        var facilitatorLink = await homePage.GetFacilitatorLinkAsync();

        await _page!.GotoAsync(facilitatorLink!);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        var facilitatorPage = new FacilitatorPage(_page, _playwrightFixture.Settings.BaseUrl);
        await facilitatorPage.AddAgendaStageAsync("Stage 1", 5);
        await facilitatorPage.AddAgendaStageAsync("Stage 2", 10);
        await facilitatorPage.AddAgendaStageAsync("Stage 3", 15);
        await facilitatorPage.StartMeetingAsync();

        // Act - Start Stage 1, then skip to Stage 3
        await facilitatorPage.StartStageAsync(0);
        await Task.Delay(1000);
        await facilitatorPage.StartStageAsync(2); // Skip Stage 2
        await Task.Delay(1000);

        // Assert - Stage 2 should remain "Not Started" or "Skipped"
        var stage2Element = await _page.QuerySelectorAsync("[data-stage-index='1'], .stage:nth-of-type(2)");
        var stage2Status = stage2Element != null ? await stage2Element.GetAttributeAsync("data-status") : null;
        
        stage2Status.Should().NotBe("active", "Stage 2 should not be active");
        stage2Status.Should().NotBe("completed", "Stage 2 should not be completed");
        
        // Stage 3 should be active
        var isStage3Active = await facilitatorPage.IsStageActiveAsync(2);
        isStage3Active.Should().BeTrue("Stage 3 should be active after starting it");
    }
}
