namespace FacilitationAssistant.Core.Entities;

public class Question
{
    public Guid Id { get; set; }
    public Guid MeetingId { get; set; }
    public Meeting Meeting { get; set; } = null!;
    
    public string Text { get; set; } = string.Empty;
    public QuestionAnswerType AnswerType { get; set; }
    public QuestionTriggerType TriggerType { get; set; }
    public Guid? AssociatedStageId { get; set; }
    public AgendaStage? AssociatedStage { get; set; }
    
    public QuestionResultVisibility ResultVisibility { get; set; } = QuestionResultVisibility.FacilitatorOnly;
    public bool AllowAnonymousAnswers { get; set; } = false;
    public QuestionStatus Status { get; set; } = QuestionStatus.Draft;
    
    public string? AuthorName { get; set; }
    public bool IsAdHoc { get; set; } = false;

    public DateTime CreatedAt { get; set; }
    public DateTime? TriggeredAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    
    // For scale questions
    public int? ScaleMin { get; set; }
    public int? ScaleMax { get; set; }
    public string? ScaleMinLabel { get; set; }
    public string? ScaleMaxLabel { get; set; }
    
    // For multiple choice questions
    public int? MaxSelectableOptions { get; set; }
    
    public List<QuestionOption> Options { get; set; } = new();
    public List<QuestionResponse> Responses { get; set; } = new();
}

public enum QuestionAnswerType
{
    SingleChoice,
    MultipleChoice,
    FreeText,
    Scale
}

public enum QuestionTriggerType
{
    MeetingStart,
    StageStart,
    Manual
}

public enum QuestionResultVisibility
{
    FacilitatorOnly,
    AggregatedResultsToAttendees,
    AllResponsesToAttendees
}

public enum QuestionStatus
{
    Draft,
    Active,
    Closed
}
