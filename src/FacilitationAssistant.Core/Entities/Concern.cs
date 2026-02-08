namespace FacilitationAssistant.Core.Entities;

public class Concern
{
    public Guid Id { get; set; }
    public Guid MeetingId { get; set; }
    public Meeting Meeting { get; set; } = null!;
    
    public string SessionId { get; set; } = string.Empty;
    public string ConcernType { get; set; } = string.Empty;
    public string? CustomText { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Status tracking
    public bool IsAcknowledged { get; set; }
    public DateTime? AcknowledgedAt { get; set; }
    
    public bool IsWithdrawn { get; set; }
    public DateTime? WithdrawnAt { get; set; }
    
    // Facilitator response
    public string? ResponseText { get; set; }
    public DateTime? RespondedAt { get; set; }
    
    // Legacy field - kept for backwards compatibility
    public bool IsDismissed { get; set; }
    public DateTime? DismissedAt { get; set; }
    
    // Navigation property for votes
    public ICollection<ConcernVote> Votes { get; set; } = new List<ConcernVote>();
}
