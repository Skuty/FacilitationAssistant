using FacilitationAssistant.Core.Entities;
using FacilitationAssistant.Core.Queries;
using FacilitationAssistant.Infrastructure.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FacilitationAssistant.Infrastructure.Handlers;

/// <summary>
/// Retrieves all stage proposals submitted by a specific attendee session for a meeting.
/// </summary>
public class GetStageProposalsBySessionHandler : IRequestHandler<GetStageProposalsBySessionQuery, IEnumerable<StageProposal>>
{
    private readonly IDbContextFactory<FacilitationDbContext> _contextFactory;

    public GetStageProposalsBySessionHandler(IDbContextFactory<FacilitationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async ValueTask<IEnumerable<StageProposal>> Handle(GetStageProposalsBySessionQuery request, CancellationToken cancellationToken)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        return await context.StageProposals
            .Where(p => p.MeetingId == request.MeetingId && p.SessionId == request.SessionId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
