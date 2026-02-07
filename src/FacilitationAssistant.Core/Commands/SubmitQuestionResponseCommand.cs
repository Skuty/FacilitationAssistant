using FacilitationAssistant.Core.Entities;
using MediatR;

namespace FacilitationAssistant.Core.Commands;

public record SubmitQuestionResponseCommand(
    Guid QuestionId,
    string AttendeeSessionId,
    List<Guid>? AnswerChoiceIds,
    string? AnswerText,
    int? AnswerScaleValue,
    QuestionResponseStatus Status
) : IRequest<Guid>;
