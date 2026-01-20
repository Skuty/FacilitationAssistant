using FacilitationAssistant.Core.Domain.Common;
using FacilitationAssistant.Core.Domain.Entities;
using FacilitationAssistant.Core.Domain.Enums;
using FacilitationAssistant.Core.Interfaces;

namespace FacilitationAssistant.Core.Domain.Aggregates;

/// <summary>
/// Meeting aggregate root. Coordinates all meeting-related operations.
/// </summary>
public class Meeting : AggregateRoot
{
    public required string Title { get; set; }
    public Guid FacilitatorKey { get; private set; }
    public MeetingState State { get; private set; } = MeetingState.NotStarted;
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public Guid? CurrentStageId { get; private set; }

    private readonly List<AgendaStage> _stages = new();
    public IReadOnlyCollection<AgendaStage> Stages => _stages.AsReadOnly();

    private readonly List<Attendee> _attendees = new();
    public IReadOnlyCollection<Attendee> Attendees => _attendees.AsReadOnly();

    private readonly List<Poll> _polls = new();
    public IReadOnlyCollection<Poll> Polls => _polls.AsReadOnly();

    private readonly List<Message> _messages = new();
    public IReadOnlyCollection<Message> Messages => _messages.AsReadOnly();

    private readonly List<Concern> _concerns = new();
    public IReadOnlyCollection<Concern> Concerns => _concerns.AsReadOnly();

    private readonly List<Note> _notes = new();
    public IReadOnlyCollection<Note> Notes => _notes.AsReadOnly();

    public Meeting()
    {
        FacilitatorKey = Guid.NewGuid();
    }

    public void Start(IDateTimeProvider clock)
    {
        if (State != MeetingState.NotStarted)
            throw new InvalidOperationException("Meeting can only be started when not started");

        if (!_stages.Any())
            throw new InvalidOperationException("Cannot start meeting without agenda stages");

        State = MeetingState.InProgress;
        StartedAt = clock.UtcNow;
        MarkAsUpdated();
    }

    public void Complete(IDateTimeProvider clock)
    {
        if (State != MeetingState.InProgress)
            throw new InvalidOperationException("Meeting can only be completed when in progress");

        State = MeetingState.Completed;
        CompletedAt = clock.UtcNow;
        MarkAsUpdated();
    }

    public void AddStage(string title, string? description, int durationMinutes, int sortOrder)
    {
        var stage = new AgendaStage
        {
            Title = title,
            Description = description,
            DurationMinutes = durationMinutes,
            SortOrder = sortOrder
        };

        _stages.Add(stage);
        MarkAsUpdated();
    }

    public void StartStage(Guid stageId, IDateTimeProvider clock)
    {
        var stage = _stages.FirstOrDefault(s => s.Id == stageId);
        if (stage == null)
            throw new ArgumentException("Stage not found", nameof(stageId));

        if (State != MeetingState.InProgress)
            throw new InvalidOperationException("Meeting must be in progress to start a stage");

        if (CurrentStageId.HasValue)
        {
            var currentStage = _stages.First(s => s.Id == CurrentStageId.Value);
            if (currentStage.Status == StageStatus.InProgress)
                currentStage.Complete(clock.UtcNow);
        }

        stage.Start(clock.UtcNow);
        CurrentStageId = stageId;
        MarkAsUpdated();
    }

    public void CompleteCurrentStage(IDateTimeProvider clock)
    {
        if (!CurrentStageId.HasValue)
            throw new InvalidOperationException("No stage is currently active");

        var stage = _stages.First(s => s.Id == CurrentStageId.Value);
        stage.Complete(clock.UtcNow);
        CurrentStageId = null;
        MarkAsUpdated();
    }

    public void AddAttendee(string sessionId, string displayName)
    {
        if (_attendees.Any(a => a.SessionId == sessionId))
            return;

        _attendees.Add(new Attendee
        {
            SessionId = sessionId,
            DisplayName = displayName
        });
        MarkAsUpdated();
    }

    public void AddPoll(Poll poll)
    {
        _polls.Add(poll);
        MarkAsUpdated();
    }

    public void AddMessage(string content, string senderSessionId, string senderName, MessageType type)
    {
        _messages.Add(new Message
        {
            Content = content,
            SenderSessionId = senderSessionId,
            SenderName = senderName,
            Type = type
        });
        MarkAsUpdated();
    }

    public void CloseMessage(Guid messageId)
    {
        var message = _messages.FirstOrDefault(m => m.Id == messageId);
        if (message == null)
            throw new ArgumentException("Message not found", nameof(messageId));

        message.Close();
        MarkAsUpdated();
    }

    public void AddConcern(string content, string raisedBySessionId, string? raisedByName, bool isAnonymous, ConcernSeverity severity)
    {
        _concerns.Add(new Concern
        {
            Content = content,
            RaisedBySessionId = raisedBySessionId,
            RaisedByName = isAnonymous ? null : raisedByName,
            IsAnonymous = isAnonymous,
            Severity = severity
        });
        MarkAsUpdated();
    }

    public void AcknowledgeConcern(Guid concernId, string? response, IDateTimeProvider clock)
    {
        var concern = _concerns.FirstOrDefault(c => c.Id == concernId);
        if (concern == null)
            throw new ArgumentException("Concern not found", nameof(concernId));

        concern.Acknowledge(response, clock.UtcNow);
        MarkAsUpdated();
    }

    public void ResolveConcern(Guid concernId, IDateTimeProvider clock)
    {
        var concern = _concerns.FirstOrDefault(c => c.Id == concernId);
        if (concern == null)
            throw new ArgumentException("Concern not found", nameof(concernId));

        concern.Resolve(clock.UtcNow);
        MarkAsUpdated();
    }

    public void AddNote(string content, string ownerId, string ownerName, bool isPrivate, Guid? linkedStageId = null)
    {
        _notes.Add(new Note
        {
            Content = content,
            OwnerId = ownerId,
            OwnerName = ownerName,
            IsPrivate = isPrivate,
            LinkedStageId = linkedStageId
        });
        MarkAsUpdated();
    }

    public void UpdateNote(Guid noteId, string newContent, IDateTimeProvider clock)
    {
        var note = _notes.FirstOrDefault(n => n.Id == noteId);
        if (note == null)
            throw new ArgumentException("Note not found", nameof(noteId));

        note.UpdateContent(newContent, clock.UtcNow);
        MarkAsUpdated();
    }

    public void DeleteNote(Guid noteId)
    {
        var note = _notes.FirstOrDefault(n => n.Id == noteId);
        if (note == null)
            throw new ArgumentException("Note not found", nameof(noteId));

        _notes.Remove(note);
        MarkAsUpdated();
    }

    public void RegenerateFacilitatorKey()
    {
        FacilitatorKey = Guid.NewGuid();
        MarkAsUpdated();
    }
}
