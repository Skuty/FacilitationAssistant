using Mediator;

namespace FacilitationAssistant.Core.Commands;

public record AcknowledgeConcernCommand(
    Guid ConcernId
) : ICommand<Unit>;
