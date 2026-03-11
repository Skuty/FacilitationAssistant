namespace FacilitationAssistant.Core.Entities;

public class Message
{
    public Guid Id { get; set; }
    public Guid MeetingId { get; set; }
    public Meeting Meeting { get; set; } = null!;
    
    public string Text { get; set; } = string.Empty;
    public MessageType Type { get; set; }
    public MessageResponseType? ResponseType { get; set; }
    public string? AuthorName { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public bool IsClosed { get; set; }
    
    // For predefined answer options
    public List<MessageOption> Options { get; set; } = new();
    public List<MessageResponse> Responses { get; set; } = new();
}

public class MessageOption
{
    public Guid Id { get; set; }
    public Guid MessageId { get; set; }
    public Message Message { get; set; } = null!;
    
    public string Text { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
}

public enum MessageType
{
    Announcement,
    Question
}

public enum MessageResponseType
{
    Reactions,
    PredefinedAnswers,
    FreeText
}
