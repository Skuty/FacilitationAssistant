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
            var query = _context.Meetings
                .Include(m => m.Stages.OrderBy(s => s.OrderIndex))
                .Include(m => m.Notes)
                .Include(m => m.Concerns)
                .Include(m => m.AttendeeSessions)
                .AsQueryable();

            Meeting? meeting;
            if (request.IsFacilitator)
            {
                meeting = await query.FirstOrDefaultAsync(m => m.FacilitatorToken == request.Token, cancellationToken);
            }
            else
            {
                meeting = await query.FirstOrDefaultAsync(m => m.AttendeeToken == request.Token, cancellationToken);
            }

            if (meeting == null)
            {
                _logger.LogWarning("Meeting not found for token (IsFacilitator: {IsFacilitator})", request.IsFacilitator);
            }
            else
            {
                _logger.LogInformation("Meeting {MeetingId} loaded successfully (IsFacilitator: {IsFacilitator})", 
                    meeting.Id, request.IsFacilitator);
            }

            return meeting;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading meeting by token (IsFacilitator: {IsFacilitator})", request.IsFacilitator);
            throw;
        }
    }
}
