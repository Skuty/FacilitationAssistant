using FacilitationAssistant.E2ETests.Utilities;

namespace FacilitationAssistant.E2ETests.PageObjects;

/// <summary>
/// Page Object for the facilitator view
/// </summary>
public class FacilitatorPage : BasePage
{
    // Selectors
    private const string AddStageButton = "[data-testid='add-stage'], button:has-text('Add Stage')";
    private const string StageNameInput = "[data-testid='stage-name'], input[placeholder*='Stage']";
    private const string StageDurationInput = "[data-testid='stage-duration'], input[type='number']";
    private const string SaveStageButton = "[data-testid='save-stage'], button:has-text('Save')";
    private const string StartMeetingButton = "[data-testid='start-meeting'], button:has-text('Start Meeting')";
    private const string EndMeetingButton = "[data-testid='end-meeting'], button:has-text('End Meeting')";
    private const string StageContainer = "[data-testid='stage-list'], .stage";
    private const string StartStageButton = "[data-testid='start-stage']";
    private const string EndStageButton = "[data-testid='end-stage'], button:has-text('End Stage')";
    private const string AddTimeButton = "[data-testid='add-time'], button:has-text('Add')";
    
    // Question/Poll selectors
    private const string CreateQuestionButton = "[data-testid='create-question'], button:has-text('Question')";
    private const string QuestionTextInput = "[data-testid='question-text'], textarea";
    private const string QuestionTypeSelect = "[data-testid='question-type'], select";
    private const string TriggerQuestionButton = "[data-testid='trigger-question'], button:has-text('Send')";
    
    // Message selectors
    private const string SendMessageButton = "[data-testid='send-message'], button:has-text('Message')";
    private const string MessageTextArea = "[data-testid='message-text'], textarea";
    private const string MessageTypeSelect = "[data-testid='message-type'], select";
    
    // Concerns selectors
    private const string ConcernsPanelButton = "[data-testid='concerns-panel'], button:has-text('Concerns')";
    private const string AcknowledgeConcernButton = "[data-testid='acknowledge-concern']";
    private const string RespondToConcernButton = "[data-testid='respond-concern']";
    
    // Notes selectors
    private const string AddNoteButton = "[data-testid='add-note'], button:has-text('Note')";
    private const string NoteTextArea = "[data-testid='note-text'], textarea";
    private const string NoteVisibilitySelect = "[data-testid='note-visibility'], select";

    public FacilitatorPage(IPage page, string baseUrl) : base(page, baseUrl)
    {
    }

    /// <summary>
    /// Navigates to facilitator page with specific meeting ID
    /// </summary>
    public async Task NavigateToMeetingAsync(string meetingId)
    {
        await base.NavigateAsync($"/facilitator/{meetingId}");
    }

    /// <summary>
    /// Adds a new agenda stage
    /// </summary>
    public async Task AddAgendaStageAsync(string name, int durationMinutes, string? description = null)
    {
        await Page.ClickAsync(AddStageButton);
        await Task.Delay(500);
        
        await Page.FillAsync(StageNameInput, name);
        await Page.FillAsync(StageDurationInput, durationMinutes.ToString());
        
        if (!string.IsNullOrEmpty(description))
        {
            var descInput = "[data-testid='stage-description'], textarea";
            if (await Page.ElementExistsAsync(descInput))
            {
                await Page.FillAsync(descInput, description);
            }
        }
        
        await Page.ClickAsync(SaveStageButton);
        await Task.Delay(500); // Wait for stage to be added
    }

    /// <summary>
    /// Starts the meeting
    /// </summary>
    public async Task StartMeetingAsync()
    {
        await Page.ClickAsync(StartMeetingButton);
        await Task.Delay(1000); // Wait for meeting to start
        await WaitForLoadAsync();
    }

    /// <summary>
    /// Ends the meeting
    /// </summary>
    public async Task EndMeetingAsync()
    {
        await Page.ClickAsync(EndMeetingButton);
        
        // Handle confirmation dialog if present
        Page.Dialog += async (_, dialog) => await dialog.AcceptAsync();
        
        await Task.Delay(1000);
    }

    /// <summary>
    /// Starts a specific stage by index
    /// </summary>
    public async Task StartStageAsync(int stageIndex = 0)
    {
        var stages = await Page.QuerySelectorAllAsync(StageContainer);
        if (stageIndex < stages.Count)
        {
            var startButton = await stages[stageIndex].QuerySelectorAsync(StartStageButton);
            if (startButton != null)
            {
                await startButton.ClickAsync();
                await Task.Delay(500);
            }
        }
    }

    /// <summary>
    /// Ends the currently active stage
    /// </summary>
    public async Task EndCurrentStageAsync()
    {
        await Page.ClickAsync(EndStageButton);
        await Task.Delay(500);
    }

    /// <summary>
    /// Gets the count of stages in the agenda
    /// </summary>
    public async Task<int> GetStageCountAsync()
    {
        var stages = await Page.QuerySelectorAllAsync(StageContainer);
        return stages.Count;
    }

    /// <summary>
    /// Checks if a stage is active
    /// </summary>
    public async Task<bool> IsStageActiveAsync(int stageIndex)
    {
        var stages = await Page.QuerySelectorAllAsync(StageContainer);
        if (stageIndex < stages.Count)
        {
            var activeIndicator = await stages[stageIndex].QuerySelectorAsync("[data-status='active'], .active");
            return activeIndicator != null;
        }
        return false;
    }

    /// <summary>
    /// Creates a new question/poll
    /// </summary>
    public async Task CreateQuestionAsync(string questionText, string type = "single-choice", string[]? options = null)
    {
        await Page.ClickAsync(CreateQuestionButton);
        await Task.Delay(500);
        
        await Page.FillAsync(QuestionTextInput, questionText);
        await Page.SelectOptionAsync(QuestionTypeSelect, type);
        
        if (options != null)
        {
            for (int i = 0; i < options.Length; i++)
            {
                var optionInput = $"[data-testid='option-{i}'], input[placeholder*='Option {i + 1}']";
                await Page.FillAsync(optionInput, options[i]);
            }
        }
        
        await Page.ClickAsync(TriggerQuestionButton);
        await Task.Delay(500);
    }

    /// <summary>
    /// Sends a message to attendees
    /// </summary>
    public async Task SendMessageAsync(string messageText, string type = "announcement")
    {
        await Page.ClickAsync(SendMessageButton);
        await Task.Delay(500);
        
        await Page.FillAsync(MessageTextArea, messageText);
        await Page.SelectOptionAsync(MessageTypeSelect, type);
        
        var sendButton = "[data-testid='send-msg-btn'], button:has-text('Send')";
        await Page.ClickAsync(sendButton);
        await Task.Delay(500);
    }

    /// <summary>
    /// Opens the concerns panel
    /// </summary>
    public async Task OpenConcernsPanelAsync()
    {
        await Page.ClickAsync(ConcernsPanelButton);
        await Task.Delay(500);
    }

    /// <summary>
    /// Acknowledges a concern by index
    /// </summary>
    public async Task AcknowledgeConcernAsync(int concernIndex = 0)
    {
        var concerns = await Page.QuerySelectorAllAsync("[data-testid='concern-item'], .concern");
        if (concernIndex < concerns.Count)
        {
            var ackButton = await concerns[concernIndex].QuerySelectorAsync(AcknowledgeConcernButton);
            if (ackButton != null)
            {
                await ackButton.ClickAsync();
                await Task.Delay(500);
            }
        }
    }

    /// <summary>
    /// Adds a note
    /// </summary>
    public async Task AddNoteAsync(string noteText, string visibility = "public")
    {
        await Page.ClickAsync(AddNoteButton);
        await Task.Delay(500);
        
        await Page.FillAsync(NoteTextArea, noteText);
        await Page.SelectOptionAsync(NoteVisibilitySelect, visibility);
        
        var saveButton = "[data-testid='save-note'], button:has-text('Save')";
        await Page.ClickAsync(saveButton);
        await Task.Delay(500);
    }

    /// <summary>
    /// Gets the current meeting status
    /// </summary>
    public async Task<string?> GetMeetingStatusAsync()
    {
        var status = "[data-testid='meeting-status'], .meeting-status";
        return await GetTextAsync(status);
    }

    /// <summary>
    /// Gets the number of connected attendees
    /// </summary>
    public async Task<int> GetAttendeeCountAsync()
    {
        var countElement = "[data-testid='attendee-count'], .attendee-count";
        var text = await GetTextAsync(countElement);
        
        if (string.IsNullOrEmpty(text))
            return 0;
        
        // Extract number from text like "5 attendees"
        var match = System.Text.RegularExpressions.Regex.Match(text, @"\d+");
        return match.Success ? int.Parse(match.Value) : 0;
    }
}
