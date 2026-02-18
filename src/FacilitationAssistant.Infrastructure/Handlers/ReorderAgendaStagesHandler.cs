using FacilitationAssistant.Core.Commands;
using FacilitationAssistant.Core.Entities;
using FacilitationAssistant.Infrastructure.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FacilitationAssistant.Infrastructure.Handlers;

public class ReorderAgendaStagesHandler : IRequestHandler<ReorderAgendaStagesCommand, bool>
{
    private readonly FacilitationDbContext _context;

    public ReorderAgendaStagesHandler(FacilitationDbContext context)
    {
        _context = context;
    }

    public async ValueTask<bool> Handle(ReorderAgendaStagesCommand request, CancellationToken cancellationToken)
    {
        var meeting = await _context.Meetings
            .FirstOrDefaultAsync(m => m.Id == request.MeetingId, cancellationToken);

        if (meeting == null || meeting.Status != MeetingStatus.Setup)
        {
            return false;
        }

        var stageToMove = await _context.AgendaStages
            .FirstOrDefaultAsync(s => s.Id == request.StageId, cancellationToken);
        
        if (stageToMove == null)
        {
            return false;
        }

        var currentIndex = stageToMove.OrderIndex;
        var newIndex = request.NewOrderIndex;

        var stageCount = await _context.AgendaStages
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
        var sortedStages = await _context.AgendaStages
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

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
