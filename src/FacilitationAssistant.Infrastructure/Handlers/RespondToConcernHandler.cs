using FacilitationAssistant.Core.Commands;
using FacilitationAssistant.Infrastructure.Data;
using FacilitationAssistant.Infrastructure.Hubs;
using Mediator;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Text.Encodings.Web;

namespace FacilitationAssistant.Infrastructure.Handlers;

/// <summary>
/// Handles responding to a raised concern.
/// </summary>
public class RespondToConcernHandler : ICommandHandler<RespondToConcernCommand>
{
    private readonly IDbContextFactory<FacilitationDbContext> _contextFactory;
    private readonly IHubContext<MeetingHub> _hubContext;

    public RespondToConcernHandler(
        IDbContextFactory<FacilitationDbContext> contextFactory,
        IHubContext<MeetingHub> hubContext)
    {
        _contextFactory = contextFactory;
        _hubContext = hubContext;
    }

    public async ValueTask<Unit> Handle(RespondToConcernCommand request, CancellationToken cancellationToken)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        
        var concern = await context.Concerns
            .FirstOrDefaultAsync(c => c.Id == request.ConcernId, cancellationToken);
            
        if (concern == null)
            throw new InvalidOperationException("Concern not found");

        concern.ResponseText = string.IsNullOrWhiteSpace(request.ResponseText) ? null : HtmlEncoder.Default.Encode(request.ResponseText);
        concern.RespondedAt = DateTime.UtcNow;
        
        // Automatically acknowledge when responding
        if (!concern.IsAcknowledged)
        {
            concern.IsAcknowledged = true;
            concern.AcknowledgedAt = DateTime.UtcNow;
        }

        await context.SaveChangesAsync(cancellationToken);
        
        // Notify all clients in the meeting group
        await _hubContext.Clients.Group(concern.MeetingId.ToString())
            .SendAsync("MeetingUpdated", "concern_responded", cancellationToken);
        
        return Unit.Value;
    }
}
