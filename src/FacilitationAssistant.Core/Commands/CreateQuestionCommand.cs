using FacilitationAssistant.Core.Entities;
using Mediator;

namespace FacilitationAssistant.Core.Commands;

public record CreateQuestionCommand(
    Guid MeetingId,
    string Text,
    QuestionAnswerType AnswerType,
    QuestionTriggerType TriggerType,
    Guid? AssociatedStageId,
    QuestionResultVisibility ResultVisibility,
    List<string>? Options,
    int? ScaleMin,
    int? ScaleMax,
    string? ScaleMinLabel,
    string? ScaleMaxLabel,
    int? MaxSelectableOptions,
    bool TriggerImmediately
) : IRequest<Guid>;
