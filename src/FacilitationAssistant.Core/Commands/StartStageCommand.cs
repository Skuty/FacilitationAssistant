using MediatR;

namespace FacilitationAssistant.Core.Commands;

public record StartStageCommand(Guid MeetingId, Guid StageId) : IRequest<Unit>;
