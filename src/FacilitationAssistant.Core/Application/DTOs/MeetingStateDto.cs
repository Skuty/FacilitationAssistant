using FacilitationAssistant.Core.Domain.Enums;

namespace FacilitationAssistant.Core.Application.DTOs;

/// <summary>
/// DTO representing complete meeting state for public broadcast.
/// </summary>
public record MeetingStateDto
{
    public required Guid Id { get; init; }
    public required string Title { get; init; }
    public required MeetingState State { get; init; }
    public DateTime? StartedAt { get; init; }
    public Guid? CurrentStageId { get; init; }
    public required List<AgendaStageDto> Stages { get; init; }
    public required List<AttendeeDto> Attendees { get; init; }
    public required List<PollDto> Polls { get; init; }
    public required List<MessageDto> Messages { get; init; }
    public required List<ConcernDto> Concerns { get; init; }
    public required List<NoteDto> PublicNotes { get; init; }
}

public record AgendaStageDto
{
    public required Guid Id { get; init; }
    public required string Title { get; init; }
    public string? Description { get; init; }
    public required int DurationMinutes { get; init; }
    public required StageStatus Status { get; init; }
    public DateTime? StartedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
}

public record AttendeeDto
{
    public required Guid Id { get; init; }
    public required string DisplayName { get; init; }
    public required DateTime JoinTime { get; init; }
}

public record PollDto
{
    public required Guid Id { get; init; }
    public required string Question { get; init; }
    public required PollStatus Status { get; init; }
    public required bool AllowMultipleVotes { get; init; }
    public required bool ShowResultsBeforeClose { get; init; }
    public required List<PollOptionDto> Options { get; init; }
    public required List<VoteDto> Votes { get; init; }
}

public record PollOptionDto
{
    public required Guid Id { get; init; }
    public required string Text { get; init; }
    public required int VoteCount { get; init; }
}

public record VoteDto
{
    public required Guid Id { get; init; }
    public required Guid OptionId { get; init; }
}

public record MessageDto
{
    public required Guid Id { get; init; }
    public required string Content { get; init; }
    public required string SenderName { get; init; }
    public required MessageType Type { get; init; }
    public required bool IsClosed { get; init; }
    public required DateTime Timestamp { get; init; }
}

public record ConcernDto
{
    public required Guid Id { get; init; }
    public required string Content { get; init; }
    public string? RaisedByName { get; init; }
    public required ConcernSeverity Severity { get; init; }
    public required ConcernStatus Status { get; init; }
    public string? FacilitatorResponse { get; init; }
}

public record NoteDto
{
    public required Guid Id { get; init; }
    public required string Content { get; init; }
    public required string OwnerName { get; init; }
    public required bool IsPrivate { get; init; }
    public Guid? LinkedStageId { get; init; }
    public required DateTime CreatedAt { get; init; }
    public DateTime? LastEditedAt { get; init; }
}
