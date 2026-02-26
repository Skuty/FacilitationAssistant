using FacilitationAssistant.Core.Commands;
using FacilitationAssistant.Core.Entities;
using FacilitationAssistant.Infrastructure.Data;
using FacilitationAssistant.Infrastructure.Hubs;
using Mediator;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace FacilitationAssistant.Infrastructure.Handlers;

/// <summary>
/// Handles updating an agenda stage's editable fields (name, description, planned duration).
/// Permitted in Setup mode for any stage, and in Active meeting mode for NotStarted or Active stages.
/// </summary>
public class UpdateAgendaStageHandler : IRequestHandler<UpdateAgendaStageCommand, bool>
{
    private readonly IDbContextFactory<FacilitationDbContext> _contextFactory;
    private readonly IHubContext<MeetingHub> _hubContext;

    public UpdateAgendaStageHandler(
        IDbContextFactory<FacilitationDbContext> contextFactory,
        IHubContext<MeetingHub> hubContext)
    {
        _contextFactory = contextFactory;
        _hubContext = hubContext;
    }

    public async ValueTask<bool> Handle(UpdateAgendaStageCommand request, CancellationToken cancellationToken)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var meeting = await context.Meetings
            .FirstOrDefaultAsync(m => m.Id == request.MeetingId, cancellationToken);

        if (meeting == null)
            throw new InvalidOperationException("Meeting not found");

        var stage = await context.AgendaStages
            .FirstOrDefaultAsync(s => s.Id == request.StageId && s.MeetingId == request.MeetingId, cancellationToken);

        if (stage == null)
            throw new InvalidOperationException("Stage not found");

        // Completed and Skipped stages cannot be edited
        if (stage.Status == StageStatus.Completed || stage.Status == StageStatus.Skipped)
            throw new InvalidOperationException("Cannot edit a stage that has already been completed or skipped");

        // For Active stages, only planned duration is adjustable (name/description changes also permitted)
        stage.Name = request.Name.Trim();
        stage.Description = string.IsNullOrWhiteSpace(request.Description) ? string.Empty : request.Description.Trim();
        stage.PlannedDurationMinutes = request.PlannedDurationMinutes;

        await context.SaveChangesAsync(cancellationToken);

        // Broadcast update so attendee views refresh
        await _hubContext.Clients.Group(request.MeetingId.ToString())
            .SendAsync("MeetingUpdated", "stage_updated", cancellationToken);

        return true;
    }
}
