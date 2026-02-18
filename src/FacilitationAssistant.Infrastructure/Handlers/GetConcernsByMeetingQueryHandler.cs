using FacilitationAssistant.Core.Entities;
using FacilitationAssistant.Core.Queries;
using FacilitationAssistant.Infrastructure.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FacilitationAssistant.Infrastructure.Handlers;

/// <summary>
/// Retrieves all concerns for a meeting
/// </summary>
public class GetConcernsByMeetingQueryHandler : IQueryHandler<GetConcernsByMeetingQuery, IReadOnlyList<Concern>>
{
    private readonly IDbContextFactory<FacilitationDbContext> _contextFactory;

    public GetConcernsByMeetingQueryHandler(IDbContextFactory<FacilitationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async ValueTask<IReadOnlyList<Concern>> Handle(GetConcernsByMeetingQuery request, CancellationToken cancellationToken)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        
        var concerns = await context.Concerns
            .Where(c => c.MeetingId == request.MeetingId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);

        return concerns.AsReadOnly();
    }
}
