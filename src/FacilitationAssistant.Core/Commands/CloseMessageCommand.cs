using Mediator;

namespace FacilitationAssistant.Core.Commands;

public record CloseMessageCommand(
    Guid MessageId
) : IRequest<Unit>;
