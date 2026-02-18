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
        
        return await context.Questions
            .Where(q => q.MeetingId == request.MeetingId)
            .OrderBy(q => q.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
