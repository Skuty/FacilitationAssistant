using Mediator;

namespace FacilitationAssistant.Core.Commands;

public record ProposeStageCommand(
    Guid MeetingId,
    string SessionId,
    string? ProposerName,
    string Name,
    string? Description,
    int PlannedDurationMinutes
) : IRequest<Guid>;
