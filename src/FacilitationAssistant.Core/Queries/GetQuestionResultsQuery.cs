using MediatR;

namespace FacilitationAssistant.Core.Queries;

public record GetQuestionResultsQuery(Guid QuestionId) : IRequest<QuestionResultsDto?>;

public record QuestionResultsDto(
    int TotalResponses,
    int TotalSkipped,
    int TotalPending,
    Dictionary<Guid, int>? ChoiceResults,
    List<string>? FreeTextAnswers,
    ScaleResultsDto? ScaleResults
);

public record ScaleResultsDto(
    double AverageScore,
    Dictionary<int, int> Distribution
);
