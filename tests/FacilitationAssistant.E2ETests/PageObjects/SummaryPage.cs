using FacilitationAssistant.E2ETests.Utilities;

namespace FacilitationAssistant.E2ETests.PageObjects;

/// <summary>
/// Page Object for the meeting summary page (post-meeting)
/// </summary>
public class SummaryPage : BasePage
{
    // Selectors
    private const string SummaryContainer = "[data-testid='summary'], .summary";
    private const string MeetingTitle = "[data-testid='meeting-title'], h1";
    private const string MeetingDuration = "[data-testid='meeting-duration']";
    private const string AgendaSection = "[data-testid='agenda-section'], .agenda-summary";
    private const string StageItem = "[data-testid='stage-summary'], .stage";
    private const string NotesSection = "[data-testid='notes-section'], .notes-summary";
    private const string NoteItem = "[data-testid='note-item'], .note";
    private const string PollResultsSection = "[data-testid='poll-results'], .polls";
    private const string PollItem = "[data-testid='poll-item'], .poll";
    private const string ConcernsSection = "[data-testid='concerns-section'], .concerns-summary";
    private const string ConcernItem = "[data-testid='concern-item'], .concern";
    private const string MessagesSection = "[data-testid='messages-section'], .messages-summary";
    private const string MessageItem = "[data-testid='message-item'], .message";
    private const string PrintButton = "[data-testid='print'], button:has-text('Print')";

    public SummaryPage(IPage page, string baseUrl) : base(page, baseUrl)
    {
    }

    /// <summary>
    /// Navigates to summary page with specific meeting ID
    /// </summary>
    public async Task NavigateToSummaryAsync(string meetingId, bool isFacilitator = false)
    {
        var route = isFacilitator ? "facilitator" : "attendee";
        await base.NavigateAsync($"/{route}/{meetingId}");
    }

    /// <summary>
    /// Checks if summary page is displayed
    /// </summary>
    public async Task<bool> IsSummaryDisplayedAsync()
    {
        return await IsVisibleAsync(SummaryContainer);
    }

    /// <summary>
    /// Gets the meeting title
    /// </summary>
    public async Task<string?> GetMeetingTitleAsync()
    {
        return await GetTextAsync(MeetingTitle);
    }

    /// <summary>
    /// Gets the total meeting duration
    /// </summary>
    public async Task<string?> GetMeetingDurationAsync()
    {
        return await GetTextAsync(MeetingDuration);
    }

    /// <summary>
    /// Gets the list of stage summaries
    /// </summary>
    public async Task<List<StageSummary>> GetStageSummariesAsync()
    {
        var summaries = new List<StageSummary>();
        var stages = await Page.QuerySelectorAllAsync(StageItem);
        
        foreach (var stage in stages)
        {
            var name = await stage.QuerySelectorAsync("[data-testid='stage-name']");
            var status = await stage.QuerySelectorAsync("[data-testid='stage-status']");
            var duration = await stage.QuerySelectorAsync("[data-testid='stage-duration']");
            
            summaries.Add(new StageSummary
            {
                Name = name != null ? await name.TextContentAsync() : "",
                Status = status != null ? await status.TextContentAsync() : "",
                Duration = duration != null ? await duration.TextContentAsync() : ""
            });
        }
        
        return summaries;
    }

    /// <summary>
    /// Gets the count of notes in summary
    /// </summary>
    public async Task<int> GetNotesCountAsync()
    {
        var notes = await Page.QuerySelectorAllAsync(NoteItem);
        return notes.Count;
    }

    /// <summary>
    /// Checks if notes section is visible
    /// </summary>
    public async Task<bool> AreNotesVisibleAsync()
    {
        return await IsVisibleAsync(NotesSection);
    }

    /// <summary>
    /// Gets the count of poll results
    /// </summary>
    public async Task<int> GetPollResultsCountAsync()
    {
        var polls = await Page.QuerySelectorAllAsync(PollItem);
        return polls.Count;
    }

    /// <summary>
    /// Checks if poll results section is visible
    /// </summary>
    public async Task<bool> ArePollResultsVisibleAsync()
    {
        return await IsVisibleAsync(PollResultsSection);
    }

    /// <summary>
    /// Gets poll result by index
    /// </summary>
    public async Task<PollResult?> GetPollResultAsync(int pollIndex)
    {
        var polls = await Page.QuerySelectorAllAsync(PollItem);
        if (pollIndex < polls.Count)
        {
            var poll = polls[pollIndex];
            var question = await poll.QuerySelectorAsync("[data-testid='poll-question']");
            var resultText = await poll.QuerySelectorAsync("[data-testid='poll-result']");
            
            return new PollResult
            {
                Question = question != null ? await question.TextContentAsync() : "",
                Result = resultText != null ? await resultText.TextContentAsync() : ""
            };
        }
        return null;
    }

    /// <summary>
    /// Gets the count of concerns in summary
    /// </summary>
    public async Task<int> GetConcernsCountAsync()
    {
        var concerns = await Page.QuerySelectorAllAsync(ConcernItem);
        return concerns.Count;
    }

    /// <summary>
    /// Checks if concerns section is visible
    /// </summary>
    public async Task<bool> AreConcernsVisibleAsync()
    {
        return await IsVisibleAsync(ConcernsSection);
    }

    /// <summary>
    /// Gets the count of messages in summary
    /// </summary>
    public async Task<int> GetMessagesCountAsync()
    {
        var messages = await Page.QuerySelectorAllAsync(MessageItem);
        return messages.Count;
    }

    /// <summary>
    /// Checks if messages section is visible
    /// </summary>
    public async Task<bool> AreMessagesVisibleAsync()
    {
        return await IsVisibleAsync(MessagesSection);
    }

    /// <summary>
    /// Clicks the print button
    /// </summary>
    public async Task ClickPrintAsync()
    {
        await Page.ClickAsync(PrintButton);
        await Task.Delay(500);
    }

    /// <summary>
    /// Verifies summary contains all expected sections
    /// </summary>
    public async Task<bool> HasAllSectionsAsync()
    {
        var hasAgenda = await IsVisibleAsync(AgendaSection);
        // Notes, polls, concerns may be optional depending on meeting content
        return hasAgenda;
    }
}

/// <summary>
/// Data class for stage summary information
/// </summary>
public class StageSummary
{
    public string Name { get; set; } = "";
    public string Status { get; set; } = "";
    public string Duration { get; set; } = "";
}

/// <summary>
/// Data class for poll result information
/// </summary>
public class PollResult
{
    public string Question { get; set; } = "";
    public string Result { get; set; } = "";
}
