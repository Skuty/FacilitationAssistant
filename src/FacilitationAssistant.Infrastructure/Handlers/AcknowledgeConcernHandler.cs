using FacilitationAssistant.Core.Commands;
using FacilitationAssistant.Infrastructure.Data;
using FacilitationAssistant.Infrastructure.Hubs;
using Mediator;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace FacilitationAssistant.Infrastructure.Handlers;

/// <summary>
/// Handles acknowledging a concern raised during a meeting.
/// </summary>
public class AcknowledgeConcernHandler : ICommandHandler<AcknowledgeConcernCommand>
{
    private readonly IDbContextFactory<FacilitationDbContext> _contextFactory;
    private readonly IHubContext<MeetingHub> _hubContext;

    public AcknowledgeConcernHandler(
        IDbContextFactory<FacilitationDbContext> contextFactory,
        IHubContext<MeetingHub> hubContext)
    {
        _contextFactory = contextFactory;
        _hubContext = hubContext;
    }

    public async ValueTask<Unit> Handle(AcknowledgeConcernCommand request, CancellationToken cancellationToken)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        
        var concern = await context.Concerns
            .FirstOrDefaultAsync(c => c.Id == request.ConcernId, cancellationToken);
            
        if (concern == null)
            throw new InvalidOperationException("Concern not found");

        concern.IsAcknowledged = true;
        concern.AcknowledgedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);
        
        // Notify all clients in the meeting group
        await _hubContext.Clients.Group(concern.MeetingId.ToString())
            .SendAsync("MeetingUpdated", "concern_acknowledged", cancellationToken);
        
        return Unit.Value;
    }
}
