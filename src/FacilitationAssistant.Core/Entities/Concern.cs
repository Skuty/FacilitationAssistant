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
    public bool IsDismissed { get; set; }
    public DateTime? DismissedAt { get; set; }
    public int VotesUp { get; set; }
    public int VotesDown { get; set; }
}
