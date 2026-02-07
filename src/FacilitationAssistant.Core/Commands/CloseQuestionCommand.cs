using Mediator;

namespace FacilitationAssistant.Core.Commands;

public record CloseQuestionCommand(
    Guid QuestionId
) : IRequest<bool>;
