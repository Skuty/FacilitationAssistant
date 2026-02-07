using FacilitationAssistant.Core.Entities;
using FacilitationAssistant.Core.Queries;
using FacilitationAssistant.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FacilitationAssistant.Infrastructure.Handlers;

public class GetMeetingByTokenHandler : IRequestHandler<GetMeetingByTokenQuery, Meeting?>
{
    private readonly FacilitationDbContext _context;

    public GetMeetingByTokenHandler(FacilitationDbContext context)
    {
        _context = context;
    }

    public async Task<Meeting?> Handle(GetMeetingByTokenQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Meetings
            .Include(m => m.Stages.OrderBy(s => s.OrderIndex))
            .Include(m => m.Notes)
            .Include(m => m.Concerns)
            .Include(m => m.AttendeeSessions)
            .AsQueryable();

        if (request.IsFacilitator)
        {
            return await query.FirstOrDefaultAsync(m => m.FacilitatorToken == request.Token, cancellationToken);
        }
        else
        {
            return await query.FirstOrDefaultAsync(m => m.AttendeeToken == request.Token, cancellationToken);
        }
    }
}
