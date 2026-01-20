using FacilitationAssistant.Core.Domain.Common;

namespace FacilitationAssistant.Core.Domain.Entities;

/// <summary>
/// Represents an attendee participant in a meeting.
/// </summary>
public class Attendee : Entity
{
    public required string SessionId { get; init; }
    public required string DisplayName { get; init; }
    public DateTime JoinTime { get; init; } = DateTime.UtcNow;
}
