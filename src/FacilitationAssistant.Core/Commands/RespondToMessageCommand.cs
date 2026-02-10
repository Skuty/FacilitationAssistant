using Mediator;

namespace FacilitationAssistant.Core.Commands;

public record RespondToMessageCommand(
    Guid MessageId,
    string SessionId,
    string? Reaction,
    Guid? SelectedOptionId,
    string? FreeText
) : IRequest<Guid>;
