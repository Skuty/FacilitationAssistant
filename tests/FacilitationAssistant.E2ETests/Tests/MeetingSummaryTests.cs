using FacilitationAssistant.E2ETests.Fixtures;
using FacilitationAssistant.E2ETests.PageObjects;

namespace FacilitationAssistant.E2ETests.Tests;

/// <summary>
/// P1 (High) tests for Meeting Summary & Post-Meeting Access
/// Tests meeting end, summary generation, and summary content
/// </summary>
[Collection("E2E")]
[Trait("Priority", "P1")]
[Trait("Feature", "MeetingSummary")]
public class MeetingSummaryTests : IAsyncLifetime
{
    private readonly PlaywrightFixture _playwrightFixture;
    private readonly WebApplicationFixture _webAppFixture;
    private IBrowserContext? _context;
    private IPage? _page;

    public MeetingSummaryTests(PlaywrightFixture playwrightFixture, WebApplicationFixture webAppFixture)
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
    public async Task P1_070_EndMeeting_ShouldGenerateSummary()
    {
        // Arrange - Create and run a meeting
        var homePage = new HomePage(_page!, _playwrightFixture.Settings.BaseUrl);
        await homePage.NavigateAsync();
        await homePage.ClickCreateMeetingAsync();
        var facilitatorLink = await homePage.GetFacilitatorLinkAsync();

        await _page!.GotoAsync(facilitatorLink!);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        var facilitatorPage = new FacilitatorPage(_page, _playwrightFixture.Settings.BaseUrl);
        await facilitatorPage.AddAgendaStageAsync("Stage 1", 5);
        await facilitatorPage.StartMeetingAsync();
        await facilitatorPage.StartStageAsync(0);
        await Task.Delay(2000);

        // Act - End the meeting
        await facilitatorPage.EndMeetingAsync();
        await Task.Delay(2000);

        // Assert - Should redirect to summary
        var summaryPage = new SummaryPage(_page, _playwrightFixture.Settings.BaseUrl);
        var summaryDisplayed = await summaryPage.IsSummaryDisplayedAsync();
        summaryDisplayed.Should().BeTrue("summary should be displayed after meeting ends");
    }

    [Fact]
    public async Task P1_071_Summary_ShouldShowAgendaOverview()
    {
        // Arrange - Create meeting with multiple stages
        var homePage = new HomePage(_page!, _playwrightFixture.Settings.BaseUrl);
        await homePage.NavigateAsync();
        await homePage.ClickCreateMeetingAsync();
        var facilitatorLink = await homePage.GetFacilitatorLinkAsync();

        await _page!.GotoAsync(facilitatorLink!);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        var facilitatorPage = new FacilitatorPage(_page, _playwrightFixture.Settings.BaseUrl);
        await facilitatorPage.AddAgendaStageAsync("Introduction", 5);
        await facilitatorPage.AddAgendaStageAsync("Discussion", 10);
        await facilitatorPage.StartMeetingAsync();
        await facilitatorPage.StartStageAsync(0);
        await Task.Delay(2000);
        await facilitatorPage.EndCurrentStageAsync();
        await facilitatorPage.EndMeetingAsync();
        await Task.Delay(2000);

        // Act - View summary
        var summaryPage = new SummaryPage(_page, _playwrightFixture.Settings.BaseUrl);
        var stageSummaries = await summaryPage.GetStageSummariesAsync();

        // Assert - Should show all stages with status
        stageSummaries.Should().NotBeEmpty("summary should contain stage information");
        stageSummaries.Should().HaveCountGreaterThanOrEqualTo(1, "at least one stage should be in summary");
    }

    [Fact]
    public async Task P1_072_Summary_FacilitatorView_ShouldShowAllNotes()
    {
        // Arrange - Create meeting and add notes
        var homePage = new HomePage(_page!, _playwrightFixture.Settings.BaseUrl);
        await homePage.NavigateAsync();
        await homePage.ClickCreateMeetingAsync();
        var facilitatorLink = await homePage.GetFacilitatorLinkAsync();

        await _page!.GotoAsync(facilitatorLink!);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        var facilitatorPage = new FacilitatorPage(_page, _playwrightFixture.Settings.BaseUrl);
        await facilitatorPage.AddAgendaStageAsync("Stage 1", 5);
        await facilitatorPage.StartMeetingAsync();
        
        // Add notes
        await facilitatorPage.AddNoteAsync("Public note", "public");
        await facilitatorPage.AddNoteAsync("Private note", "private");
        await Task.Delay(1000);
        
        // End meeting
        await facilitatorPage.EndMeetingAsync();
        await Task.Delay(2000);

        // Act - View summary
        var summaryPage = new SummaryPage(_page, _playwrightFixture.Settings.BaseUrl);
        var notesVisible = await summaryPage.AreNotesVisibleAsync();

        // Assert - Notes should be visible in summary
        notesVisible.Should().BeTrue("notes section should be visible in summary");
        
        var notesCount = await summaryPage.GetNotesCountAsync();
        notesCount.Should().BeGreaterThan(0, "summary should contain notes");
    }

    [Fact]
    public async Task P1_073_Summary_AttendeeView_ShouldShowOnlyPublicNotes()
    {
        // Arrange - Create meeting with public and private notes
        var homePage = new HomePage(_page!, _playwrightFixture.Settings.BaseUrl);
        await homePage.NavigateAsync();
        await homePage.ClickCreateMeetingAsync();
        var facilitatorLink = await homePage.GetFacilitatorLinkAsync();
        var attendeeLink = await homePage.GetAttendeeLinkAsync();

        // Facilitator creates notes
        await _page!.GotoAsync(facilitatorLink!);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        var facilitatorPage = new FacilitatorPage(_page, _playwrightFixture.Settings.BaseUrl);
        await facilitatorPage.AddAgendaStageAsync("Stage 1", 5);
        await facilitatorPage.StartMeetingAsync();
        await facilitatorPage.AddNoteAsync("Public note visible to all", "public");
        await facilitatorPage.AddNoteAsync("Private facilitator note", "private");
        await Task.Delay(1000);
        await facilitatorPage.EndMeetingAsync();
        await Task.Delay(2000);

        // Act - Attendee views summary
        var attendeePage = await _context!.NewPageAsync();
        await attendeePage.GotoAsync(attendeeLink!);
        await attendeePage.WaitForLoadStateAsync(LoadState.NetworkIdle);
        var attendeeSummary = new SummaryPage(attendeePage, _playwrightFixture.Settings.BaseUrl);

        // Assert - Should see public notes but not private
        var notesVisible = await attendeeSummary.AreNotesVisibleAsync();
        notesVisible.Should().BeTrue("attendee should see notes section");
        
        // Check that private note is not visible
        var pageText = await attendeePage.TextContentAsync("body");
        pageText.Should().Contain("Public note", "attendee should see public notes");
        pageText.Should().NotContain("Private facilitator note", "attendee should NOT see private notes");

        await attendeePage.CloseAsync();
    }

    [Fact]
    public async Task P1_074_Summary_ShouldPersistAcrossReload()
    {
        // Arrange - End a meeting
        var homePage = new HomePage(_page!, _playwrightFixture.Settings.BaseUrl);
        await homePage.NavigateAsync();
        await homePage.ClickCreateMeetingAsync();
        var facilitatorLink = await homePage.GetFacilitatorLinkAsync();

        await _page!.GotoAsync(facilitatorLink!);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        var facilitatorPage = new FacilitatorPage(_page, _playwrightFixture.Settings.BaseUrl);
        await facilitatorPage.AddAgendaStageAsync("Stage 1", 5);
        await facilitatorPage.StartMeetingAsync();
        await facilitatorPage.EndMeetingAsync();
        await Task.Delay(2000);

        // Get current URL
        var summaryUrl = _page.Url;

        // Act - Reload the page
        await _page.ReloadAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Assert - Summary should still be visible
        var summaryPage = new SummaryPage(_page, _playwrightFixture.Settings.BaseUrl);
        var summaryDisplayed = await summaryPage.IsSummaryDisplayedAsync();
        summaryDisplayed.Should().BeTrue("summary should persist after page reload");
    }
}
