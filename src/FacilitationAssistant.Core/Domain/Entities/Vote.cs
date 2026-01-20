using FacilitationAssistant.Core.Domain.Common;

namespace FacilitationAssistant.Core.Domain.Entities;

/// <summary>
/// Represents a vote cast by an attendee on a poll option.
/// </summary>
public class Vote : Entity
{
    public required string VoterSessionId { get; init; }
    public required Guid OptionId { get; init; }
}
