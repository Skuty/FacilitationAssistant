using MediatR;

namespace FacilitationAssistant.Core.Commands;

public record ReorderAgendaStagesCommand(
    Guid MeetingId,
    Guid StageId,
    int NewOrderIndex
) : IRequest<bool>;
