using FacilitationAssistant.Core.Entities;
using FacilitationAssistant.Core.Queries;
using FacilitationAssistant.Infrastructure.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FacilitationAssistant.Infrastructure.Handlers;

/// <summary>
/// Retrieves all questions for a meeting
/// </summary>
public class GetQuestionsByMeetingHandler : IRequestHandler<GetQuestionsByMeetingQuery, List<Question>>
{
    private readonly IDbContextFactory<FacilitationDbContext> _contextFactory;

    public GetQuestionsByMeetingHandler(IDbContextFactory<FacilitationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async ValueTask<List<Question>> Handle(GetQuestionsByMeetingQuery request, CancellationToken cancellationToken)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var questions = await context.Questions
            .Where(q => q.MeetingId == request.MeetingId)
            .OrderBy(q => q.CreatedAt)
            .ToListAsync(cancellationToken);

        if (questions.Count > 0)
        {
            var questionIds = questions.Select(q => q.Id).ToHashSet();

            // Load options separately — works for InMemory, SQL Server, and Cosmos DB
            var allOptions = await context.QuestionOptions
                .Where(o => questionIds.Contains(o.QuestionId))
                .ToListAsync(cancellationToken);

            foreach (var question in questions)
            {
                question.Options = allOptions
                    .Where(o => o.QuestionId == question.Id)
                    .ToList();
            }
        }

        return questions;
    }
}
