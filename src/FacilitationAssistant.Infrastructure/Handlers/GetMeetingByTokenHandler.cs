using FacilitationAssistant.Core.Entities;
using FacilitationAssistant.Core.Queries;
using FacilitationAssistant.Infrastructure.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FacilitationAssistant.Infrastructure.Handlers;

public class GetMeetingByTokenHandler : IRequestHandler<GetMeetingByTokenQuery, Meeting?>
{
    private readonly FacilitationDbContext _context;
    private readonly ILogger<GetMeetingByTokenHandler> _logger;

    public GetMeetingByTokenHandler(FacilitationDbContext context, ILogger<GetMeetingByTokenHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async ValueTask<Meeting?> Handle(GetMeetingByTokenQuery request, CancellationToken cancellationToken)
    {
        try
        {
            Meeting? meeting;
            if (request.IsFacilitator)
            {
                meeting = await _context.Meetings
                    .FirstOrDefaultAsync(m => m.FacilitatorToken == request.Token, cancellationToken);
            }
            else
            {
                meeting = await _context.Meetings
                    .FirstOrDefaultAsync(m => m.AttendeeToken == request.Token, cancellationToken);
            }

            if (meeting == null)
            {
                _logger.LogWarning("Meeting not found for token (IsFacilitator: {IsFacilitator})", request.IsFacilitator);
                return null;
            }

            // Manually load related entities (separate containers in Cosmos DB)
            meeting.Stages = await _context.AgendaStages
                .Where(s => s.MeetingId == meeting.Id)
                .OrderBy(s => s.OrderIndex)
                .ToListAsync(cancellationToken);

            if (request.IsFacilitator)
            {
                meeting.Notes = await _context.Notes
                    .Where(n => n.MeetingId == meeting.Id)
                    .OrderByDescending(n => n.CreatedAt)
                    .ToListAsync(cancellationToken);
            }

            _logger.LogInformation("Meeting {MeetingId} loaded successfully with {StageCount} stages (IsFacilitator: {IsFacilitator})", 
                meeting.Id, meeting.Stages.Count, request.IsFacilitator);

            return meeting;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading meeting by token (IsFacilitator: {IsFacilitator})", request.IsFacilitator);
            throw;
        }
    }
}
