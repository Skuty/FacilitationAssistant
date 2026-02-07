using MediatR;

namespace FacilitationAssistant.Core.Commands;

public record EndStageCommand(Guid MeetingId, Guid StageId) : IRequest<Unit>;
