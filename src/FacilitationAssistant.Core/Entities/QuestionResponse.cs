namespace FacilitationAssistant.Core.Entities;

public class QuestionResponse
{
    public Guid Id { get; set; }
    public Guid QuestionId { get; set; }
    public Question Question { get; set; } = null!;
    
    public string AttendeeSessionId { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
    public QuestionResponseStatus Status { get; set; } = QuestionResponseStatus.Submitted;
    
    // Different answer types
    public List<string> AnswerChoiceIds { get; set; } = new();
    public string? AnswerText { get; set; }
    public int? AnswerScaleValue { get; set; }
}

public enum QuestionResponseStatus
{
    Submitted,
    Skipped
}
