using FacilitationAssistant.Core.Commands;
using FacilitationAssistant.Core.Entities;
using FacilitationAssistant.Infrastructure.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FacilitationAssistant.Infrastructure.Handlers;

/// <summary>
/// Adds a new agenda stage to a meeting
/// </summary>
public class AddAgendaStageHandler : IRequestHandler<AddAgendaStageCommand, Guid>
{
    private readonly IDbContextFactory<FacilitationDbContext> _contextFactory;

    public AddAgendaStageHandler(IDbContextFactory<FacilitationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async ValueTask<Guid> Handle(AddAgendaStageCommand request, CancellationToken cancellationToken)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        
        var meeting = await context.Meetings
            .FirstOrDefaultAsync(m => m.Id == request.MeetingId, cancellationToken);

        if (meeting == null)
            throw new InvalidOperationException("Meeting not found");

        if (meeting.Status != MeetingStatus.Setup)
            throw new InvalidOperationException("Cannot add stages to a started meeting");

        var stage = new AgendaStage
        {
            Id = Guid.NewGuid(),
            MeetingId = request.MeetingId,
            Name = request.Name,
            Description = string.IsNullOrWhiteSpace(request.Description) ? string.Empty : request.Description,
            PlannedDurationMinutes = request.PlannedDurationMinutes,
            OrderIndex = request.OrderIndex,
            Status = StageStatus.NotStarted
        };

        context.AgendaStages.Add(stage);
        await context.SaveChangesAsync(cancellationToken);

        return stage.Id;
    }
}
