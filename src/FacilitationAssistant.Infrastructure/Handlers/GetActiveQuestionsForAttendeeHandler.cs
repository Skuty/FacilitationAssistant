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
        var activeQuestions = await context.Questions
            .Where(q => q.MeetingId == request.MeetingId && q.Status == QuestionStatus.Active)
            .ToListAsync(cancellationToken);

        // Get questions already answered by this attendee
        var answeredQuestionIds = await context.QuestionResponses
            .Where(r => r.AttendeeSessionId == request.AttendeeSessionId)
            .Select(r => r.QuestionId)
            .ToListAsync(cancellationToken);

        var answeredSet = answeredQuestionIds.ToHashSet();

        // Return only questions not yet answered by this attendee
        return activeQuestions
            .Where(q => !answeredSet.Contains(q.Id))
            .OrderBy(q => q.TriggeredAt)
            .ToList();
    }
}
