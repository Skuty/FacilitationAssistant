using FacilitationAssistant.Core.Commands;
using FacilitationAssistant.Core.Entities;
using FacilitationAssistant.Infrastructure.Data;
using FacilitationAssistant.Infrastructure.Hubs;
using Mediator;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace FacilitationAssistant.Infrastructure.Handlers;

/// <summary>
/// Ends a meeting and notifies all participants
/// </summary>
public class EndMeetingHandler : IRequestHandler<EndMeetingCommand, Unit>
{
    private readonly IDbContextFactory<FacilitationDbContext> _contextFactory;
    private readonly IHubContext<MeetingHub> _hubContext;

    public EndMeetingHandler(
        IDbContextFactory<FacilitationDbContext> contextFactory, 
        IHubContext<MeetingHub> hubContext)
    {
        _contextFactory = contextFactory;
        _hubContext = hubContext;
    }

    public async ValueTask<Unit> Handle(EndMeetingCommand request, CancellationToken cancellationToken)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        
        var meeting = await context.Meetings
            .FirstOrDefaultAsync(m => m.Id == request.MeetingId, cancellationToken);

        if (meeting == null)
            throw new InvalidOperationException("Meeting not found");

        if (meeting.Status == MeetingStatus.Ended)
            return Unit.Value; // Already ended

        // End any active stage
        var activeStage = await context.AgendaStages
            .FirstOrDefaultAsync(s => s.MeetingId == request.MeetingId && s.Status == StageStatus.Active, cancellationToken);
        
        if (activeStage != null)
        {
            activeStage.Status = StageStatus.Completed;
            activeStage.CompletedAt = DateTime.UtcNow;
            activeStage.ActualDurationSeconds = (int)(activeStage.CompletedAt.Value - activeStage.StartedAt!.Value).TotalSeconds;
        }

        // Update meeting status
        meeting.Status = MeetingStatus.Ended;
        meeting.EndedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);
        
        // Notify all clients
        await _hubContext.Clients.Group(meeting.Id.ToString())
            .SendAsync("MeetingUpdated", "ended", cancellationToken);
        
        return Unit.Value;
    }
}
