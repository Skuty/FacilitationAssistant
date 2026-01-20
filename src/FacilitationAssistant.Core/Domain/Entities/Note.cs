using FacilitationAssistant.Core.Domain.Common;

namespace FacilitationAssistant.Core.Domain.Entities;

/// <summary>
/// Represents a note created during the meeting (by facilitator or attendee).
/// </summary>
public class Note : Entity
{
    public required string Content { get; set; }
    public required string OwnerId { get; init; }
    public required string OwnerName { get; init; }
    public bool IsPrivate { get; set; }
    public Guid? LinkedStageId { get; init; }
    public DateTime? LastEditedAt { get; set; }

    public void UpdateContent(string newContent, DateTime editTime)
    {
        Content = newContent;
        LastEditedAt = editTime;
    }
}
