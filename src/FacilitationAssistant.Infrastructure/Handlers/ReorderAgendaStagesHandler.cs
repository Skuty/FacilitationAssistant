using FacilitationAssistant.Core.Commands;
using FacilitationAssistant.Core.Entities;
using FacilitationAssistant.Infrastructure.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FacilitationAssistant.Infrastructure.Handlers;

/// <summary>
/// Handles reordering agenda stages in a meeting.
/// </summary>
public class ReorderAgendaStagesHandler : IRequestHandler<ReorderAgendaStagesCommand, bool>
{
    private readonly IDbContextFactory<FacilitationDbContext> _contextFactory;

    public ReorderAgendaStagesHandler(IDbContextFactory<FacilitationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async ValueTask<bool> Handle(ReorderAgendaStagesCommand request, CancellationToken cancellationToken)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        
        var meeting = await context.Meetings
            .FirstOrDefaultAsync(m => m.Id == request.MeetingId, cancellationToken);

        if (meeting == null || meeting.Status != MeetingStatus.Setup)
        {
            return false;
        }

        var stageToMove = await context.AgendaStages
            .FirstOrDefaultAsync(s => s.Id == request.StageId, cancellationToken);
        
        if (stageToMove == null)
        {
            return false;
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
        return true;
    }
}
