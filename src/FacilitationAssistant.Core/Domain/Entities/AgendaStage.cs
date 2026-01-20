using FacilitationAssistant.Core.Domain.Common;
using FacilitationAssistant.Core.Domain.Enums;

namespace FacilitationAssistant.Core.Domain.Entities;

/// <summary>
/// Represents a stage in the meeting agenda.
/// </summary>
public class AgendaStage : Entity
{
    public required string Title { get; set; }
    public string? Description { get; set; }
    public int DurationMinutes { get; set; }
    public int SortOrder { get; set; }
    public StageStatus Status { get; set; } = StageStatus.Pending;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    private readonly List<Note> _notes = new();
    public IReadOnlyCollection<Note> Notes => _notes.AsReadOnly();

    public void Start(DateTime startTime)
    {
        if (Status != StageStatus.Pending)
            throw new InvalidOperationException("Stage can only be started when pending");

        Status = StageStatus.InProgress;
        StartedAt = startTime;
    }

    public void Complete(DateTime completionTime)
    {
        if (Status != StageStatus.InProgress)
            throw new InvalidOperationException("Stage can only be completed when in progress");

        Status = StageStatus.Completed;
        CompletedAt = completionTime;
    }

    public void AddNote(Note note)
    {
        _notes.Add(note);
    }
}
