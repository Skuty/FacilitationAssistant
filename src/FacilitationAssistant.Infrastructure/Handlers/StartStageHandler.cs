using FacilitationAssistant.Core.Commands;
using FacilitationAssistant.Core.Entities;
using FacilitationAssistant.Infrastructure.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FacilitationAssistant.Infrastructure.Handlers;

public class StartStageHandler : IRequestHandler<StartStageCommand, Unit>
{
    private readonly FacilitationDbContext _context;

    public StartStageHandler(FacilitationDbContext context)
    {
        _context = context;
    }

    public async ValueTask<Unit> Handle(StartStageCommand request, CancellationToken cancellationToken)
    {
        var meeting = await _context.Meetings
            .FirstOrDefaultAsync(m => m.Id == request.MeetingId, cancellationToken);

        if (meeting == null)
            throw new InvalidOperationException("Meeting not found");

        // End any currently active stage
        var activeStage = await _context.AgendaStages
            .FirstOrDefaultAsync(s => s.MeetingId == request.MeetingId && s.Status == StageStatus.Active, cancellationToken);
        
        if (activeStage != null)
        {
            activeStage.Status = StageStatus.Completed;
            activeStage.CompletedAt = DateTime.UtcNow;
            activeStage.ActualDurationSeconds = (int)(DateTime.UtcNow - activeStage.StartedAt!.Value).TotalSeconds;
        }

        // Start the new stage
        var stage = await _context.AgendaStages
            .FirstOrDefaultAsync(s => s.Id == request.StageId, cancellationToken);
        
        if (stage == null)
            throw new InvalidOperationException("Stage not found");

        stage.Status = StageStatus.Active;
        stage.StartedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
