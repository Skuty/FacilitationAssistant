using FacilitationAssistant.Core.Entities;
using FacilitationAssistant.Core.Queries;
using FacilitationAssistant.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FacilitationAssistant.Infrastructure.Handlers;

public class GetMeetingByIdHandler : IRequestHandler<GetMeetingByIdQuery, Meeting?>
{
    private readonly FacilitationDbContext _context;

    public GetMeetingByIdHandler(FacilitationDbContext context)
    {
        _context = context;
    }

    public async Task<Meeting?> Handle(GetMeetingByIdQuery request, CancellationToken cancellationToken)
    {
        return await _context.Meetings
            .Include(m => m.Stages.OrderBy(s => s.OrderIndex))
            .Include(m => m.Notes)
            .Include(m => m.Concerns)
            .Include(m => m.AttendeeSessions)
            .FirstOrDefaultAsync(m => m.Id == request.MeetingId, cancellationToken);
    }
}
