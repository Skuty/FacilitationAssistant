using Mediator;

namespace FacilitationAssistant.Core.Commands;

public record RaiseConcernCommand(
    Guid MeetingId,
    string SessionId,
    string ConcernType,
    string? CustomText = null
) : IRequest<Guid>;
