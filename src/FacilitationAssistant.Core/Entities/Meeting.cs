namespace FacilitationAssistant.Core.Entities;

public class Meeting
{
    public Guid Id { get; set; }
    public string FacilitatorToken { get; set; } = string.Empty;
    public string AttendeeToken { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public int? TotalPlannedDurationMinutes { get; set; }
    public MeetingStatus Status { get; set; } = MeetingStatus.Setup;
    
    public List<AgendaStage> Stages { get; set; } = new();
    public List<Note> Notes { get; set; } = new();
    public List<Concern> Concerns { get; set; } = new();
    public List<AttendeeSession> AttendeeSessions { get; set; } = new();
    public List<Question> Questions { get; set; } = new();
    public List<Message> Messages { get; set; } = new();
}

public enum MeetingStatus
{
    Setup,
    Active,
    Ended
}
