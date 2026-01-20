using FacilitationAssistant.Core.Domain.Common;
using FacilitationAssistant.Core.Domain.Enums;
using MediatR;

namespace FacilitationAssistant.Core.Application.Commands;

/// <summary>
/// Command to send a message in the meeting.
/// </summary>
public record SendMessageCommand(
    Guid MeetingId,
    string Content,
    string SenderSessionId,
    string SenderName,
    MessageType Type
) : IRequest<Result<Guid>>;
