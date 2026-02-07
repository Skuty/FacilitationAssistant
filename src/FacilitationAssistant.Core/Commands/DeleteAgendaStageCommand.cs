using MediatR;

namespace FacilitationAssistant.Core.Commands;

public record DeleteAgendaStageCommand(
    Guid MeetingId,
    Guid StageId
) : IRequest<Unit>;
