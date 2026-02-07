using Mediator;

namespace FacilitationAssistant.Core.Commands;

public record TriggerQuestionCommand(
    Guid QuestionId
) : IRequest<bool>;
