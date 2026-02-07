namespace FacilitationAssistant.Core.Entities;

public class QuestionOption
{
    public Guid Id { get; set; }
    public Guid QuestionId { get; set; }
    public Question Question { get; set; } = null!;
    
    public string OptionText { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
}
