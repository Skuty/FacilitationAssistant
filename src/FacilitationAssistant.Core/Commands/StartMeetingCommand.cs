using MediatR;

namespace FacilitationAssistant.Core.Commands;

public record StartMeetingCommand(Guid MeetingId) : IRequest<Unit>;
