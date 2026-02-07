using Mediator;

namespace FacilitationAssistant.Core.Commands;

public record EndMeetingCommand(Guid MeetingId) : IRequest<Unit>;
