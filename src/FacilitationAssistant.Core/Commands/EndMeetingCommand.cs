using MediatR;

namespace FacilitationAssistant.Core.Commands;

public record EndMeetingCommand(Guid MeetingId) : IRequest<Unit>;
