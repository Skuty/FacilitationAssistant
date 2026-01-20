using FacilitationAssistant.Core.Domain.Common;
using MediatR;

namespace FacilitationAssistant.Core.Application.Commands;

/// <summary>
/// Command to join a meeting as an attendee.
/// </summary>
public record JoinMeetingCommand(
    Guid MeetingId,
    string SessionId,
    string DisplayName
) : IRequest<Result>;
