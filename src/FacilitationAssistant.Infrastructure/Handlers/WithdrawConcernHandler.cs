using FacilitationAssistant.Core.Commands;
using FacilitationAssistant.Infrastructure.Data;
using FacilitationAssistant.Infrastructure.Hubs;
using Mediator;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace FacilitationAssistant.Infrastructure.Handlers;

/// <summary>
/// Handles withdrawing a concern that was previously raised.
/// </summary>
public class WithdrawConcernHandler : ICommandHandler<WithdrawConcernCommand>
{
    private readonly IDbContextFactory<FacilitationDbContext> _contextFactory;
    private readonly IHubContext<MeetingHub> _hubContext;

    public WithdrawConcernHandler(
        IDbContextFactory<FacilitationDbContext> contextFactory,
        IHubContext<MeetingHub> hubContext)
    {
        _contextFactory = contextFactory;
        _hubContext = hubContext;
    }

    public async ValueTask<Unit> Handle(WithdrawConcernCommand request, CancellationToken cancellationToken)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        
        var concern = await context.Concerns
            .FirstOrDefaultAsync(c => c.Id == request.ConcernId, cancellationToken);
            
        if (concern == null)
            throw new InvalidOperationException("Concern not found");

        // Only the person who raised the concern can withdraw it
        if (concern.SessionId != request.SessionId)
        {
            throw new InvalidOperationException("You can only withdraw your own concerns");
        }

        concern.IsWithdrawn = true;
        concern.WithdrawnAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);
        
        // Notify all clients in the meeting group
        await _hubContext.Clients.Group(concern.MeetingId.ToString())
            .SendAsync("MeetingUpdated", "concern_withdrawn", cancellationToken);
        
        return Unit.Value;
    }
}
