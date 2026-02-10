namespace FacilitationAssistant.Core.Entities;

public class MessageResponse
{
    public Guid Id { get; set; }
    public Guid MessageId { get; set; }
    public Message Message { get; set; } = null!;
    
    public string SessionId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    
    // For reactions
    public string? Reaction { get; set; }
    
    // For predefined answers
    public Guid? SelectedOptionId { get; set; }
    
    // For free text
    public string? FreeText { get; set; }
}
