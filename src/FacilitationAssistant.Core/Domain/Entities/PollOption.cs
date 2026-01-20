using FacilitationAssistant.Core.Domain.Common;

namespace FacilitationAssistant.Core.Domain.Entities;

/// <summary>
/// Represents an option in a poll.
/// </summary>
public class PollOption : Entity
{
    public required string Text { get; set; }
    public int SortOrder { get; init; }
}
