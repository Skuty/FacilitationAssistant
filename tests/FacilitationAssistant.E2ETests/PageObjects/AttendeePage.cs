using FacilitationAssistant.E2ETests.Utilities;

namespace FacilitationAssistant.E2ETests.PageObjects;

/// <summary>
/// Page Object for the attendee view
/// </summary>
public class AttendeePage : BasePage
{
    // Selectors
    private const string WaitingMessage = "[data-testid='waiting-message'], .waiting";
    private const string AgendaContainer = "[data-testid='agenda'], .agenda";
    private const string StageContainer = "[data-testid='stage-item'], .stage";
    private const string ActiveStageIndicator = "[data-status='active'], .active";
    private const string TimerDisplay = "[data-testid='timer'], .timer";
    
    // Question selectors
    private const string QuestionModal = "[data-testid='question-modal'], .question";
    private const string QuestionText = "[data-testid='question-text']";
    private const string AnswerOption = "[data-testid='answer-option'], input[type='radio'], input[type='checkbox']";
    private const string SubmitAnswerButton = "[data-testid='submit-answer'], button:has-text('Submit')";
    private const string SkipQuestionButton = "[data-testid='skip-question'], button:has-text('Skip')";
    
    // Concerns selectors
    private const string RaiseConcernButton = "[data-testid='raise-concern'], button:has-text('Concern')";
    private const string ConcernTypeSelect = "[data-testid='concern-type'], select";
    private const string CustomConcernInput = "[data-testid='custom-concern'], textarea";
    private const string SubmitConcernButton = "[data-testid='submit-concern'], button:has-text('Submit')";
    private const string ConcernItem = "[data-testid='concern-item'], .concern";
    private const string LikeButton = "[data-testid='like'], button:has-text('👍')";
    private const string DislikeButton = "[data-testid='dislike'], button:has-text('👎')";
    private const string WithdrawConcernButton = "[data-testid='withdraw-concern']";
    
    // Notes selectors
    private const string NotesPanel = "[data-testid='notes-panel']";
    private const string AddNoteButton = "[data-testid='add-note'], button:has-text('Note')";
    private const string NoteTextArea = "[data-testid='note-text'], textarea";
    private const string SaveNoteButton = "[data-testid='save-note'], button:has-text('Save')";
    
    // Messages selectors
    private const string MessageNotification = "[data-testid='message-notification'], .notification";
    private const string MessageText = "[data-testid='message-text']";
    private const string DismissButton = "[data-testid='dismiss'], button:has-text('Dismiss')";
    private const string ReactButton = "[data-testid='react']";
    
    // Connection status
    private const string ConnectionStatus = "[data-testid='connection-status'], .connection";

    public AttendeePage(IPage page, string baseUrl) : base(page, baseUrl)
    {
    }

    /// <summary>
    /// Navigates to attendee page with specific meeting ID
    /// </summary>
    public async Task NavigateToMeetingAsync(string meetingId)
    {
        await base.NavigateAsync($"/attendee/{meetingId}");
    }

    /// <summary>
    /// Waits for the meeting to start
    /// </summary>
    public async Task<bool> WaitForMeetingStartAsync(int timeoutMs = 10000)
    {
        try
        {
            // Wait for agenda to appear (meeting started)
            await Page.WaitForSelectorAsync(AgendaContainer, new PageWaitForSelectorOptions 
            { 
                State = WaitForSelectorState.Visible,
                Timeout = timeoutMs 
            });
            return true;
        }
        catch (TimeoutException)
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if waiting message is displayed
    /// </summary>
    public async Task<bool> IsWaitingMessageDisplayedAsync()
    {
        return await IsVisibleAsync(WaitingMessage);
    }

    /// <summary>
    /// Gets the currently active stage text
    /// </summary>
    public async Task<string?> GetActiveStageTextAsync()
    {
        try
        {
            var activeStage = await Page.QuerySelectorAsync(ActiveStageIndicator);
            if (activeStage != null)
            {
                var stageName = await activeStage.QuerySelectorAsync("[data-testid='stage-name']");
                return stageName != null ? await stageName.TextContentAsync() : null;
            }
            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Gets the timer display text
    /// </summary>
    public async Task<string?> GetTimerTextAsync()
    {
        return await GetTextAsync(TimerDisplay);
    }

    /// <summary>
    /// Checks if a question is displayed
    /// </summary>
    public async Task<bool> IsQuestionDisplayedAsync()
    {
        return await IsVisibleAsync(QuestionModal);
    }

    /// <summary>
    /// Gets the question text
    /// </summary>
    public async Task<string?> GetQuestionTextAsync()
    {
        return await GetTextAsync(QuestionText);
    }

    /// <summary>
    /// Answers a single-choice question by option index
    /// </summary>
    public async Task AnswerSingleChoiceAsync(int optionIndex)
    {
        var options = await Page.QuerySelectorAllAsync(AnswerOption);
        if (optionIndex < options.Count)
        {
            await options[optionIndex].ClickAsync();
            await Page.ClickAsync(SubmitAnswerButton);
            await Task.Delay(500);
        }
    }

    /// <summary>
    /// Answers a free-text question
    /// </summary>
    public async Task AnswerFreeTextAsync(string answerText)
    {
        var textarea = "[data-testid='free-text-answer'], textarea";
        await Page.FillAsync(textarea, answerText);
        await Page.ClickAsync(SubmitAnswerButton);
        await Task.Delay(500);
    }

    /// <summary>
    /// Skips the current question
    /// </summary>
    public async Task SkipQuestionAsync()
    {
        await Page.ClickAsync(SkipQuestionButton);
        await Task.Delay(500);
    }

    /// <summary>
    /// Raises a predefined concern
    /// </summary>
    public async Task RaisePredefinedConcernAsync(string concernType)
    {
        await Page.ClickAsync(RaiseConcernButton);
        await Task.Delay(500);
        
        await Page.SelectOptionAsync(ConcernTypeSelect, concernType);
        await Page.ClickAsync(SubmitConcernButton);
        await Task.Delay(500);
    }

    /// <summary>
    /// Raises a custom concern
    /// </summary>
    public async Task RaiseCustomConcernAsync(string concernText)
    {
        await Page.ClickAsync(RaiseConcernButton);
        await Task.Delay(500);
        
        await Page.SelectOptionAsync(ConcernTypeSelect, "custom");
        await Page.FillAsync(CustomConcernInput, concernText);
        await Page.ClickAsync(SubmitConcernButton);
        await Task.Delay(500);
    }

    /// <summary>
    /// Votes on a concern (like/dislike/neutral)
    /// </summary>
    public async Task VoteOnConcernAsync(int concernIndex, string voteType = "like")
    {
        var concerns = await Page.QuerySelectorAllAsync(ConcernItem);
        if (concernIndex < concerns.Count)
        {
            var voteButton = voteType.ToLower() switch
            {
                "dislike" => await concerns[concernIndex].QuerySelectorAsync(DislikeButton),
                "neutral" => await concerns[concernIndex].QuerySelectorAsync("[data-testid='neutral']"),
                _ => await concerns[concernIndex].QuerySelectorAsync(LikeButton)
            };
            
            if (voteButton != null)
            {
                await voteButton.ClickAsync();
                await Task.Delay(500);
            }
        }
    }

    /// <summary>
    /// Withdraws a concern that was raised by this attendee
    /// </summary>
    public async Task WithdrawConcernAsync(int concernIndex)
    {
        var concerns = await Page.QuerySelectorAllAsync(ConcernItem);
        if (concernIndex < concerns.Count)
        {
            var withdrawButton = await concerns[concernIndex].QuerySelectorAsync(WithdrawConcernButton);
            if (withdrawButton != null)
            {
                await withdrawButton.ClickAsync();
                await Task.Delay(500);
            }
        }
    }

    /// <summary>
    /// Gets the count of concerns displayed
    /// </summary>
    public async Task<int> GetConcernCountAsync()
    {
        var concerns = await Page.QuerySelectorAllAsync(ConcernItem);
        return concerns.Count;
    }

    /// <summary>
    /// Creates a note
    /// </summary>
    public async Task CreateNoteAsync(string noteText)
    {
        await Page.ClickAsync(AddNoteButton);
        await Task.Delay(500);
        
        await Page.FillAsync(NoteTextArea, noteText);
        await Page.ClickAsync(SaveNoteButton);
        await Task.Delay(500);
    }

    /// <summary>
    /// Checks if a message notification is displayed
    /// </summary>
    public async Task<bool> IsMessageNotificationDisplayedAsync()
    {
        return await IsVisibleAsync(MessageNotification);
    }

    /// <summary>
    /// Gets the message text from notification
    /// </summary>
    public async Task<string?> GetMessageTextAsync()
    {
        return await GetTextAsync(MessageText);
    }

    /// <summary>
    /// Dismisses a message notification
    /// </summary>
    public async Task DismissMessageAsync()
    {
        await Page.ClickAsync(DismissButton);
        await Task.Delay(500);
    }

    /// <summary>
    /// Reacts to a message with a specific reaction
    /// </summary>
    public async Task ReactToMessageAsync(string reaction = "👍")
    {
        var reactionButton = $"{ReactButton}:has-text('{reaction}')";
        await Page.ClickAsync(reactionButton);
        await Task.Delay(500);
    }

    /// <summary>
    /// Gets the connection status
    /// </summary>
    public async Task<string?> GetConnectionStatusAsync()
    {
        return await GetTextAsync(ConnectionStatus);
    }

    /// <summary>
    /// Waits for stage sync (checks if active stage updates)
    /// </summary>
    public async Task<bool> WaitForStageSyncAsync(string expectedStageName, int timeoutMs = 5000)
    {
        return await BrowserHelpers.WaitForConditionAsync(async () =>
        {
            var activeStage = await GetActiveStageTextAsync();
            return activeStage?.Contains(expectedStageName) ?? false;
        }, timeoutMs);
    }
}
