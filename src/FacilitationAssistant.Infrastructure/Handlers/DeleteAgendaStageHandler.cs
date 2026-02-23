using FacilitationAssistant.Core.Commands;
using FacilitationAssistant.Core.Entities;
using FacilitationAssistant.Infrastructure.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FacilitationAssistant.Infrastructure.Handlers;

/// <summary>
/// Handles deleting an agenda stage from a meeting.
/// </summary>
public class DeleteAgendaStageHandler : IRequestHandler<DeleteAgendaStageCommand, Unit>
{
    private readonly IDbContextFactory<FacilitationDbContext> _contextFactory;

    public DeleteAgendaStageHandler(IDbContextFactory<FacilitationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async ValueTask<Unit> Handle(DeleteAgendaStageCommand request, CancellationToken cancellationToken)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        
        var meeting = await context.Meetings
            .FirstOrDefaultAsync(m => m.Id == request.MeetingId, cancellationToken);

        if (meeting == null)
            throw new InvalidOperationException("Meeting not found");

        var stage = await context.AgendaStages
            .FirstOrDefaultAsync(s => s.Id == request.StageId, cancellationToken);
        
        if (stage == null)
            throw new InvalidOperationException("Stage not found");

        // Only allow deletion of stages that haven't started yet
        if (stage.Status != StageStatus.NotStarted)
            throw new InvalidOperationException("Cannot delete a stage that has already started or been completed");

        // Load notes and questions attached to this stage so EF's ClientSetNull
        // behaviour can null out their StageId/AssociatedStageId before the stage
        // row is deleted. (ClientSetNull only affects tracked entities.)
        await context.Notes
            .Where(n => n.StageId == request.StageId)
            .LoadAsync(cancellationToken);

        await context.Questions
            .Where(q => q.AssociatedStageId == request.StageId)
            .LoadAsync(cancellationToken);

        context.AgendaStages.Remove(stage);
        
        // Reorder remaining stages to close the gap
        var remainingStages = await context.AgendaStages
            .Where(s => s.MeetingId == request.MeetingId && s.OrderIndex > stage.OrderIndex)
            .OrderBy(s => s.OrderIndex)
            .ToListAsync(cancellationToken);

        foreach (var remainingStage in remainingStages)
        {
            remainingStage.OrderIndex--;
        }

        await context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
