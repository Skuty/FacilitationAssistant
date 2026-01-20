using FacilitationAssistant.Core.Domain.Common;
using FacilitationAssistant.Core.Domain.Enums;

namespace FacilitationAssistant.Core.Domain.Entities;

/// <summary>
/// Represents a message (announcement or question) in the meeting.
/// </summary>
public class Message : Entity
{
    public required string Content { get; set; }
    public required string SenderSessionId { get; init; }
    public required string SenderName { get; init; }
    public MessageType Type { get; init; }
    public bool IsClosed { get; set; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    public void Close()
    {
        if (IsClosed)
            throw new InvalidOperationException("Message is already closed");

        IsClosed = true;
    }
}
