using FacilitationAssistant.Core.Domain.Common;
using MediatR;

namespace FacilitationAssistant.Core.Application.Commands;

/// <summary>
/// Command to start a meeting.
/// </summary>
public record StartMeetingCommand(Guid MeetingId) : IRequest<Result>;
