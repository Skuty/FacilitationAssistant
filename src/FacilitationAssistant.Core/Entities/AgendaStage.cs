namespace FacilitationAssistant.Core.Entities;

public class AgendaStage
{
    public Guid Id { get; set; }
    public Guid MeetingId { get; set; }
    public Meeting Meeting { get; set; } = null!;
    
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int PlannedDurationMinutes { get; set; }
    public int OrderIndex { get; set; }
    
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public StageStatus Status { get; set; } = StageStatus.NotStarted;
    public int? ActualDurationSeconds { get; set; }
}

public enum StageStatus
{
    NotStarted,
    Active,
    Completed,
    Skipped
}
