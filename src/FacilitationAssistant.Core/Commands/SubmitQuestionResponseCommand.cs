using FacilitationAssistant.Core.Entities;
using Mediator;

namespace FacilitationAssistant.Core.Commands;

public record SubmitQuestionResponseCommand(
    Guid QuestionId,
    string AttendeeSessionId,
    List<string>? AnswerChoiceIds,
    string? AnswerText,
    int? AnswerScaleValue,
    QuestionResponseStatus Status
) : IRequest<Guid>;
