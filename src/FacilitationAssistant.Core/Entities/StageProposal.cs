namespace FacilitationAssistant.Core.Entities;

public class StageProposal
{
    public Guid Id { get; set; }
    public Guid MeetingId { get; set; }
    public Meeting Meeting { get; set; } = null!;

    /// <summary>Session ID of the attendee who submitted the proposal.</summary>
    public string SessionId { get; set; } = string.Empty;

    /// <summary>Display name of the proposer (may be null if anonymous).</summary>
    public string? ProposerName { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int PlannedDurationMinutes { get; set; }

    public StageProposalStatus Status { get; set; } = StageProposalStatus.Pending;

    public DateTime CreatedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }

    /// <summary>Optional facilitator note when rejecting.</summary>
    public string? RejectionReason { get; set; }

    /// <summary>The AgendaStage that was created when this proposal was accepted (null until accepted).</summary>
    public Guid? AcceptedStageId { get; set; }
}

public enum StageProposalStatus
{
    Pending,
    Accepted,
    Rejected
}
