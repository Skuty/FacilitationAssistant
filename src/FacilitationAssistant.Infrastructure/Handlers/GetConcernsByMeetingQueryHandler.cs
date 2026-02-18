using FacilitationAssistant.Core.Entities;
using FacilitationAssistant.Core.Queries;
using FacilitationAssistant.Infrastructure.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FacilitationAssistant.Infrastructure.Handlers;

public class GetConcernsByMeetingQueryHandler : IQueryHandler<GetConcernsByMeetingQuery, IReadOnlyList<Concern>>
{
    private readonly FacilitationDbContext _context;

    public GetConcernsByMeetingQueryHandler(FacilitationDbContext context)
    {
        _context = context;
    }

    public async ValueTask<IReadOnlyList<Concern>> Handle(GetConcernsByMeetingQuery request, CancellationToken cancellationToken)
    {
        var concerns = await _context.Concerns
            .Where(c => c.MeetingId == request.MeetingId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);

        return concerns.AsReadOnly();
    }
}
