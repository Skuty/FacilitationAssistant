using FacilitationAssistant.Core.Commands;
using FacilitationAssistant.Core.Entities;
using FacilitationAssistant.Infrastructure.Data;
using FacilitationAssistant.Infrastructure.Hubs;
using Mediator;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace FacilitationAssistant.Infrastructure.Handlers;

/// <summary>
/// Handles starting an agenda stage.
/// </summary>
public class StartStageHandler : IRequestHandler<StartStageCommand, Unit>
{
    private readonly IDbContextFactory<FacilitationDbContext> _contextFactory;
    private readonly IHubContext<MeetingHub> _hubContext;

    public StartStageHandler(IDbContextFactory<FacilitationDbContext> contextFactory, IHubContext<MeetingHub> hubContext)
    {
        _contextFactory = contextFactory;
        _hubContext = hubContext;
    }

    public async ValueTask<Unit> Handle(StartStageCommand request, CancellationToken cancellationToken)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        
        var meeting = await context.Meetings
            .FirstOrDefaultAsync(m => m.Id == request.MeetingId, cancellationToken);

        if (meeting == null)
            throw new InvalidOperationException("Meeting not found");

        // End any currently active stage
        var activeStage = await context.AgendaStages
            .FirstOrDefaultAsync(s => s.MeetingId == request.MeetingId && s.Status == StageStatus.Active, cancellationToken);
        
        if (activeStage != null)
        {
            activeStage.Status = StageStatus.Completed;
            activeStage.CompletedAt = DateTime.UtcNow;
            activeStage.ActualDurationSeconds = (int)(DateTime.UtcNow - activeStage.StartedAt!.Value).TotalSeconds;

            // Auto-trigger StageEnd questions for the stage being auto-completed
            var stageEndQuestions = await context.Questions
                .Where(q => q.MeetingId == request.MeetingId
                         && q.TriggerType == QuestionTriggerType.StageEnd
                         && q.AssociatedStageId == activeStage.Id
                         && q.Status == QuestionStatus.Draft)
                .ToListAsync(cancellationToken);

            foreach (var q in stageEndQuestions)
            {
                q.Status = QuestionStatus.Active;
                q.TriggeredAt = DateTime.UtcNow;
            }
        }

        // Start the new stage
        var stage = await context.AgendaStages
            .FirstOrDefaultAsync(s => s.Id == request.StageId, cancellationToken);
        
        if (stage == null)
            throw new InvalidOperationException("Stage not found");

        stage.Status = StageStatus.Active;
        stage.StartedAt = DateTime.UtcNow;

        // Auto-trigger StageStart questions for the new stage
        var stageStartQuestions = await context.Questions
            .Where(q => q.MeetingId == request.MeetingId
                     && q.TriggerType == QuestionTriggerType.StageStart
                     && q.AssociatedStageId == request.StageId
                     && q.Status == QuestionStatus.Draft)
            .ToListAsync(cancellationToken);

        foreach (var q in stageStartQuestions)
        {
            q.Status = QuestionStatus.Active;
            q.TriggeredAt = DateTime.UtcNow;
        }

        await context.SaveChangesAsync(cancellationToken);
        
        // Notify all clients
        await _hubContext.Clients.Group(request.MeetingId.ToString())
            .SendAsync("MeetingUpdated", "stage_started", cancellationToken);

        return Unit.Value;
    }
}
