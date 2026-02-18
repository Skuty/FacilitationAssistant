using FacilitationAssistant.Core.Entities;
using FacilitationAssistant.Core.Queries;
using FacilitationAssistant.Infrastructure.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FacilitationAssistant.Infrastructure.Handlers;

/// <summary>
/// Handles retrieving a meeting by its ID.
/// </summary>
public class GetMeetingByIdHandler : IRequestHandler<GetMeetingByIdQuery, Meeting?>
{
    private readonly IDbContextFactory<FacilitationDbContext> _contextFactory;

    public GetMeetingByIdHandler(IDbContextFactory<FacilitationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async ValueTask<Meeting?> Handle(GetMeetingByIdQuery request, CancellationToken cancellationToken)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        
        return await context.Meetings
            .FirstOrDefaultAsync(m => m.Id == request.MeetingId, cancellationToken);
    }
}
