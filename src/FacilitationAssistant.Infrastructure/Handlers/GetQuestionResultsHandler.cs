using FacilitationAssistant.Core.Entities;
using FacilitationAssistant.Core.Queries;
using FacilitationAssistant.Infrastructure.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FacilitationAssistant.Infrastructure.Handlers;

/// <summary>
/// Retrieves aggregated results for a question
/// </summary>
public class GetQuestionResultsHandler : IRequestHandler<GetQuestionResultsQuery, QuestionResultsDto?>
{
    private readonly IDbContextFactory<FacilitationDbContext> _contextFactory;

    public GetQuestionResultsHandler(IDbContextFactory<FacilitationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async ValueTask<QuestionResultsDto?> Handle(GetQuestionResultsQuery request, CancellationToken cancellationToken)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        
        var question = await context.Questions
            .FirstOrDefaultAsync(q => q.Id == request.QuestionId, cancellationToken);
        
        if (question == null)
            return null;

        var responses = await context.QuestionResponses
            .Where(r => r.QuestionId == request.QuestionId)
            .ToListAsync(cancellationToken);

        var options = await context.QuestionOptions
            .Where(o => o.QuestionId == request.QuestionId)
            .ToListAsync(cancellationToken);

        var totalResponses = responses.Count(r => r.Status == QuestionResponseStatus.Submitted);
        var totalSkipped = responses.Count(r => r.Status == QuestionResponseStatus.Skipped);
        
        var attendeeCount = await context.AttendeeSessions
            .CountAsync(a => a.MeetingId == question.MeetingId, cancellationToken);
        
        var totalPending = Math.Max(0, attendeeCount - totalResponses - totalSkipped);

        Dictionary<Guid, int>? choiceResults = null;
        List<FreeTextAnswerDto>? freeTextAnswers = null;
        ScaleResultsDto? scaleResults = null;

        if (question.AnswerType == QuestionAnswerType.SingleChoice || 
            question.AnswerType == QuestionAnswerType.MultipleChoice)
        {
            choiceResults = new Dictionary<Guid, int>();
            foreach (var option in options)
            {
                var optionIdStr = option.Id.ToString();
                var count = responses
                    .Where(r => r.Status == QuestionResponseStatus.Submitted)
                    .Count(r => r.AnswerChoiceIds.Contains(optionIdStr));
                choiceResults[option.Id] = count;
            }
        }
        else if (question.AnswerType == QuestionAnswerType.FreeText)
        {
            freeTextAnswers = responses
                .Where(r => r.Status == QuestionResponseStatus.Submitted && !string.IsNullOrWhiteSpace(r.AnswerText))
                .Select(r => new FreeTextAnswerDto(r.AnswerText!, r.AuthorName))
                .ToList();
        }
        else if (question.AnswerType == QuestionAnswerType.Scale)
        {
            var scaleAnswers = responses
                .Where(r => r.Status == QuestionResponseStatus.Submitted && r.AnswerScaleValue.HasValue)
                .Select(r => r.AnswerScaleValue!.Value)
                .ToList();

            if (scaleAnswers.Any())
            {
                var average = scaleAnswers.Average();
                var distribution = new Dictionary<int, int>();

                for (int i = question.ScaleMin!.Value; i <= question.ScaleMax!.Value; i++)
                {
                    distribution[i] = scaleAnswers.Count(v => v == i);
                }

                scaleResults = new ScaleResultsDto(average, distribution);
            }
        }

        return new QuestionResultsDto(
            totalResponses,
            totalSkipped,
            totalPending,
            choiceResults,
            freeTextAnswers,
            scaleResults
        );
    }
}
