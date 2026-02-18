using FacilitationAssistant.Core.Commands;
using FacilitationAssistant.Core.Entities;
using FacilitationAssistant.Infrastructure.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FacilitationAssistant.Infrastructure.Handlers;

public class DeleteAgendaStageHandler : IRequestHandler<DeleteAgendaStageCommand, Unit>
{
    private readonly FacilitationDbContext _context;

    public DeleteAgendaStageHandler(FacilitationDbContext context)
    {
        _context = context;
    }

    public async ValueTask<Unit> Handle(DeleteAgendaStageCommand request, CancellationToken cancellationToken)
    {
        var meeting = await _context.Meetings
            .FirstOrDefaultAsync(m => m.Id == request.MeetingId, cancellationToken);

        if (meeting == null)
            throw new InvalidOperationException("Meeting not found");

        var stage = await _context.AgendaStages
            .FirstOrDefaultAsync(s => s.Id == request.StageId, cancellationToken);
        
        if (stage == null)
            throw new InvalidOperationException("Stage not found");

        // Only allow deletion of stages that haven't started yet
        if (stage.Status != StageStatus.NotStarted)
            throw new InvalidOperationException("Cannot delete a stage that has already started or been completed");

        _context.AgendaStages.Remove(stage);
        
        // Reorder remaining stages to close the gap
        var remainingStages = await _context.AgendaStages
            .Where(s => s.MeetingId == request.MeetingId && s.OrderIndex > stage.OrderIndex)
            .OrderBy(s => s.OrderIndex)
            .ToListAsync(cancellationToken);

        foreach (var remainingStage in remainingStages)
        {
            remainingStage.OrderIndex--;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
