using FacilitationAssistant.Core.Commands;
using FacilitationAssistant.Core.Entities;
using FacilitationAssistant.Infrastructure.Data;
using FacilitationAssistant.Infrastructure.Hubs;
using Mediator;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace FacilitationAssistant.Infrastructure.Handlers;

/// <summary>
/// Handles reordering agenda stages in a meeting.
/// </summary>
public class ReorderAgendaStagesHandler : IRequestHandler<ReorderAgendaStagesCommand, bool>
{
    private readonly IDbContextFactory<FacilitationDbContext> _contextFactory;
    private readonly IHubContext<MeetingHub> _hubContext;

    public ReorderAgendaStagesHandler(
        IDbContextFactory<FacilitationDbContext> contextFactory,
        IHubContext<MeetingHub> hubContext)
    {
        _contextFactory = contextFactory;
        _hubContext = hubContext;
    }

    public async ValueTask<bool> Handle(ReorderAgendaStagesCommand request, CancellationToken cancellationToken)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        
        var meeting = await context.Meetings
            .FirstOrDefaultAsync(m => m.Id == request.MeetingId, cancellationToken);

        if (meeting == null || (meeting.Status != MeetingStatus.Setup && meeting.Status != MeetingStatus.Active))
        {
            return false;
        }

        var stageToMove = await context.AgendaStages
            .FirstOrDefaultAsync(s => s.Id == request.StageId, cancellationToken);
        
        if (stageToMove == null)
        {
            return false;
        }

        // For active meetings, only allow reordering NotStarted stages,
        // and only into positions that remain after all already-started stages.
        if (meeting.Status == MeetingStatus.Active)
        {
            if (stageToMove.Status != StageStatus.NotStarted)
            {
                return false;
            }

            var maxStartedIndex = await context.AgendaStages
                .Where(s => s.MeetingId == request.MeetingId && s.Status != StageStatus.NotStarted)
                .MaxAsync(s => (int?)s.OrderIndex, cancellationToken) ?? -1;

            if (request.NewOrderIndex <= maxStartedIndex)
            {
                return false;
            }
        }

        var currentIndex = stageToMove.OrderIndex;
        var newIndex = request.NewOrderIndex;

        var stageCount = await context.AgendaStages
            .CountAsync(s => s.MeetingId == request.MeetingId, cancellationToken);

        // Validate new index is within bounds
        if (newIndex < 0 || newIndex >= stageCount)
        {
            return false;
        }

        // No change needed
        if (currentIndex == newIndex)
        {
            return true;
        }

        // Reorder stages
        var sortedStages = await context.AgendaStages
            .Where(s => s.MeetingId == request.MeetingId)
            .OrderBy(s => s.OrderIndex)
            .ToListAsync(cancellationToken);
        
        // Remove from current position
        sortedStages.RemoveAt(currentIndex);
        
        // Insert at new position
        sortedStages.Insert(newIndex, stageToMove);
        
        // Update all OrderIndex values
        for (int i = 0; i < sortedStages.Count; i++)
        {
            sortedStages[i].OrderIndex = i;
        }

        await context.SaveChangesAsync(cancellationToken);

        // Notify all connected clients so attendees see the updated stage order
        await _hubContext.Clients.Group(request.MeetingId.ToString())
            .SendAsync("MeetingUpdated", "stages_reordered", cancellationToken);

        return true;
    }
}
