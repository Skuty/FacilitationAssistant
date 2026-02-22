using Mediator;

namespace FacilitationAssistant.Core.Queries;

public record GetQuestionResultsQuery(Guid QuestionId) : IRequest<QuestionResultsDto?>;

public record QuestionResultsDto(
    int TotalResponses,
    int TotalSkipped,
    int TotalPending,
    Dictionary<Guid, int>? ChoiceResults,
    List<FreeTextAnswerDto>? FreeTextAnswers,
    ScaleResultsDto? ScaleResults
);

public record FreeTextAnswerDto(
    string Answer,
    string? AuthorName
);

public record ScaleResultsDto(
    double AverageScore,
    Dictionary<int, int> Distribution
);
