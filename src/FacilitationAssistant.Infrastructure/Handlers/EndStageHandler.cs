using FacilitationAssistant.Core.Commands;
using FacilitationAssistant.Core.Entities;
using FacilitationAssistant.Infrastructure.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FacilitationAssistant.Infrastructure.Handlers;

public class EndStageHandler : IRequestHandler<EndStageCommand, Unit>
{
    private readonly FacilitationDbContext _context;

    public EndStageHandler(FacilitationDbContext context)
    {
        _context = context;
    }

    public async ValueTask<Unit> Handle(EndStageCommand request, CancellationToken cancellationToken)
    {
        var meeting = await _context.Meetings
            .FirstOrDefaultAsync(m => m.Id == request.MeetingId, cancellationToken);

        if (meeting == null)
            throw new InvalidOperationException("Meeting not found");

        var stage = await _context.AgendaStages
            .FirstOrDefaultAsync(s => s.Id == request.StageId, cancellationToken);

        if (stage == null)
            throw new InvalidOperationException("Stage not found");

        if (stage.Status != StageStatus.Active)
            throw new InvalidOperationException("Stage is not active");

        stage.Status = StageStatus.Completed;
        stage.CompletedAt = DateTime.UtcNow;
        stage.ActualDurationSeconds = (int)(DateTime.UtcNow - stage.StartedAt!.Value).TotalSeconds;

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
