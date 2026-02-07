using MediatR;

namespace FacilitationAssistant.Core.Commands;

public record AddAgendaStageCommand(
    Guid MeetingId,
    string Name,
    string? Description,
    int PlannedDurationMinutes,
    int OrderIndex
) : IRequest<Guid>;
