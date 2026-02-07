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
            .Include(m => m.Stages)
            .FirstOrDefaultAsync(m => m.Id == request.MeetingId, cancellationToken);

        if (meeting == null || meeting.Status != MeetingStatus.Setup)
        {
            return false;
        }

        var stageToMove = meeting.Stages.FirstOrDefault(s => s.Id == request.StageId);
        if (stageToMove == null)
        {
            return false;
        }

        var currentIndex = stageToMove.OrderIndex;
        var newIndex = request.NewOrderIndex;

        // Validate new index is within bounds
        if (newIndex < 0 || newIndex >= meeting.Stages.Count)
        {
            return false;
        }

        // No change needed
        if (currentIndex == newIndex)
        {
            return true;
        }

        // Reorder stages
        var sortedStages = meeting.Stages.OrderBy(s => s.OrderIndex).ToList();
        
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
