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
            .Include(m => m.Stages)
            .FirstOrDefaultAsync(m => m.Id == request.MeetingId, cancellationToken);

        if (meeting == null)
            throw new InvalidOperationException("Meeting not found");

        var stage = meeting.Stages.FirstOrDefault(s => s.Id == request.StageId);
        if (stage == null)
            throw new InvalidOperationException("Stage not found");

        // Only allow deletion of stages that haven't started yet
        if (stage.Status != StageStatus.NotStarted)
            throw new InvalidOperationException("Cannot delete a stage that has already started or been completed");

        _context.AgendaStages.Remove(stage);
        
        // Reorder remaining stages to close the gap
        var remainingStages = meeting.Stages
            .Where(s => s.Id != request.StageId && s.OrderIndex > stage.OrderIndex)
            .OrderBy(s => s.OrderIndex)
            .ToList();

        foreach (var remainingStage in remainingStages)
        {
            remainingStage.OrderIndex--;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
