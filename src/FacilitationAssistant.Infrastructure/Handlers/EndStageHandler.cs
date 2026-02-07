using FacilitationAssistant.Core.Commands;
using FacilitationAssistant.Core.Entities;
using FacilitationAssistant.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FacilitationAssistant.Infrastructure.Handlers;

public class EndStageHandler : IRequestHandler<EndStageCommand, Unit>
{
    private readonly FacilitationDbContext _context;

    public EndStageHandler(FacilitationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(EndStageCommand request, CancellationToken cancellationToken)
    {
        var stage = await _context.AgendaStages
            .FirstOrDefaultAsync(s => s.Id == request.StageId && s.MeetingId == request.MeetingId, cancellationToken);

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
