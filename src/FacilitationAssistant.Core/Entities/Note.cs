namespace FacilitationAssistant.Core.Entities;

public class Note
{
    public Guid Id { get; set; }
    public Guid MeetingId { get; set; }
    public Meeting Meeting { get; set; } = null!;
    
    public Guid? StageId { get; set; }
    public AgendaStage? Stage { get; set; }
    
    public string SessionId { get; set; } = string.Empty;
    public string? AuthorName { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsPublic { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
