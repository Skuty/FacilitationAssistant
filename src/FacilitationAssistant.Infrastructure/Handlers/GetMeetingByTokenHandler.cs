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
                meeting = await _context.Meetings.FirstOrDefaultAsync(m => m.FacilitatorToken == request.Token, cancellationToken);
            }
            else
            {
                meeting = await _context.Meetings.FirstOrDefaultAsync(m => m.AttendeeToken == request.Token, cancellationToken);
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
