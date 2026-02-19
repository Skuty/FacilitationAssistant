using FacilitationAssistant.Core.Entities;
using FacilitationAssistant.Core.Queries;
using FacilitationAssistant.Infrastructure.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FacilitationAssistant.Infrastructure.Handlers;

/// <summary>
/// Handles retrieving active questions that an attendee hasn't answered yet.
/// </summary>
public class GetActiveQuestionsForAttendeeHandler : IRequestHandler<GetActiveQuestionsForAttendeeQuery, List<Question>>
{
    private readonly IDbContextFactory<FacilitationDbContext> _contextFactory;

    public GetActiveQuestionsForAttendeeHandler(IDbContextFactory<FacilitationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async ValueTask<List<Question>> Handle(GetActiveQuestionsForAttendeeQuery request, CancellationToken cancellationToken)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        
        // Get all active questions for the meeting
        // Note: Options are loaded separately to support Cosmos DB (which ignores navigation properties across containers)
        var activeQuestions = await context.Questions
            .Where(q => q.MeetingId == request.MeetingId && q.Status == QuestionStatus.Active)
            .ToListAsync(cancellationToken);

        // Get questions already answered by this attendee
        var answeredQuestionIds = await context.QuestionResponses
            .Where(r => r.AttendeeSessionId == request.AttendeeSessionId)
            .Select(r => r.QuestionId)
            .ToListAsync(cancellationToken);

        var answeredSet = answeredQuestionIds.ToHashSet();

        var unansweredQuestions = activeQuestions
            .Where(q => !answeredSet.Contains(q.Id))
            .OrderBy(q => q.TriggeredAt)
            .ToList();

        if (unansweredQuestions.Count > 0)
        {
            var questionIds = unansweredQuestions.Select(q => q.Id).ToHashSet();

            // Load options separately — works for InMemory, SQL Server, and Cosmos DB
            var allOptions = await context.QuestionOptions
                .Where(o => questionIds.Contains(o.QuestionId))
                .ToListAsync(cancellationToken);

            foreach (var question in unansweredQuestions)
            {
                question.Options = allOptions
                    .Where(o => o.QuestionId == question.Id)
                    .ToList();
            }
        }

        return unansweredQuestions;
    }
}
