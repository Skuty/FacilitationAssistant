namespace FacilitationAssistant.Core.Entities;

public class AttendeeSession
{
    public Guid Id { get; set; }
    public Guid MeetingId { get; set; }
    public Meeting Meeting { get; set; } = null!;
    
    public string SessionId { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public DateTime JoinedAt { get; set; }
    public DateTime? LastSeenAt { get; set; }
    public bool IsConnected { get; set; }
}
