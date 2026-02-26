using Mediator;

namespace FacilitationAssistant.Core.Commands;

public record UpdateAgendaStageCommand(
    Guid MeetingId,
    Guid StageId,
    string Name,
    string? Description,
    int PlannedDurationMinutes
) : IRequest<bool>;
