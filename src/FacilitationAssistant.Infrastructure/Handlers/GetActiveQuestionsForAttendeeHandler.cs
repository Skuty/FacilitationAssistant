using FacilitationAssistant.Core.Entities;
using FacilitationAssistant.Core.Queries;
using FacilitationAssistant.Infrastructure.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FacilitationAssistant.Infrastructure.Handlers;

public class GetActiveQuestionsForAttendeeHandler : IRequestHandler<GetActiveQuestionsForAttendeeQuery, List<Question>>
{
    private readonly FacilitationDbContext _context;

    public GetActiveQuestionsForAttendeeHandler(FacilitationDbContext context)
    {
        _context = context;
    }

    public async ValueTask<List<Question>> Handle(GetActiveQuestionsForAttendeeQuery request, CancellationToken cancellationToken)
    {
        // Get all active questions for the meeting
        var activeQuestions = await _context.Questions
            .Where(q => q.MeetingId == request.MeetingId && q.Status == QuestionStatus.Active)
            .ToListAsync(cancellationToken);

        // Get questions already answered by this attendee
        var answeredQuestionIds = await _context.QuestionResponses
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
