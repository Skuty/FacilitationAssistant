using Mediator;

namespace FacilitationAssistant.Core.Commands;

public record RespondToConcernCommand(
    Guid ConcernId,
    string ResponseText
) : ICommand<Unit>;
