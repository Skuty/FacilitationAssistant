using FacilitationAssistant.Core.Domain.Common;
using FacilitationAssistant.Core.Domain.Enums;

namespace FacilitationAssistant.Core.Domain.Entities;

/// <summary>
/// Represents a concern or feedback raised during the meeting.
/// </summary>
public class Concern : Entity
{
    public required string Content { get; set; }
    public required string RaisedBySessionId { get; init; }
    public string? RaisedByName { get; init; }
    public bool IsAnonymous { get; init; }
    public ConcernSeverity Severity { get; set; }
    public ConcernStatus Status { get; set; } = ConcernStatus.Open;
    public string? FacilitatorResponse { get; set; }
    public DateTime? AcknowledgedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }

    public void Acknowledge(string? response, DateTime acknowledgeTime)
    {
        if (Status != ConcernStatus.Open)
            throw new InvalidOperationException("Can only acknowledge open concerns");

        Status = ConcernStatus.Acknowledged;
        FacilitatorResponse = response;
        AcknowledgedAt = acknowledgeTime;
    }

    public void Resolve(DateTime resolveTime)
    {
        if (Status == ConcernStatus.Resolved)
            throw new InvalidOperationException("Concern is already resolved");

        Status = ConcernStatus.Resolved;
        ResolvedAt = resolveTime;
    }
}
