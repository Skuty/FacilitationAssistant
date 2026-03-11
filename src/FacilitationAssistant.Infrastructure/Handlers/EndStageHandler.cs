using FacilitationAssistant.Core.Commands;
using FacilitationAssistant.Core.Entities;
using FacilitationAssistant.Infrastructure.Data;
using FacilitationAssistant.Infrastructure.Hubs;
using Mediator;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace FacilitationAssistant.Infrastructure.Handlers;

/// <summary>
/// Handles ending an active agenda stage.
/// </summary>
public class EndStageHandler : IRequestHandler<EndStageCommand, Unit>
{
    private readonly IDbContextFactory<FacilitationDbContext> _contextFactory;
    private readonly IHubContext<MeetingHub> _hubContext;

    public EndStageHandler(IDbContextFactory<FacilitationDbContext> contextFactory, IHubContext<MeetingHub> hubContext)
    {
        _contextFactory = contextFactory;
        _hubContext = hubContext;
    }

    public async ValueTask<Unit> Handle(EndStageCommand request, CancellationToken cancellationToken)
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

        if (stage.Status != StageStatus.Active)
            throw new InvalidOperationException("Stage is not active");

        stage.Status = StageStatus.Completed;
        stage.CompletedAt = DateTime.UtcNow;
        stage.ActualDurationSeconds = (int)(DateTime.UtcNow - stage.StartedAt!.Value).TotalSeconds;

        // Auto-trigger StageEnd questions for the ending stage
        var stageEndQuestions = await context.Questions
            .Where(q => q.MeetingId == request.MeetingId
                     && q.TriggerType == QuestionTriggerType.StageEnd
                     && q.AssociatedStageId == request.StageId
                     && q.Status == QuestionStatus.Draft)
            .ToListAsync(cancellationToken);

        foreach (var q in stageEndQuestions)
        {
            q.Status = QuestionStatus.Active;
            q.TriggeredAt = DateTime.UtcNow;
        }

        await context.SaveChangesAsync(cancellationToken);
        
        // Notify all clients
        await _hubContext.Clients.Group(request.MeetingId.ToString())
            .SendAsync("MeetingUpdated", "stage_ended", cancellationToken);

        return Unit.Value;
    }
}
