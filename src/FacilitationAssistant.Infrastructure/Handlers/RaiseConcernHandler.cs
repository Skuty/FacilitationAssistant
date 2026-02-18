using FacilitationAssistant.Core.Commands;
using FacilitationAssistant.Core.Entities;
using FacilitationAssistant.Infrastructure.Data;
using FacilitationAssistant.Infrastructure.Hubs;
using Mediator;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Text.Encodings.Web;

namespace FacilitationAssistant.Infrastructure.Handlers;

/// <summary>
/// Handles raising a new concern during a meeting.
/// </summary>
public class RaiseConcernHandler : IRequestHandler<RaiseConcernCommand, Guid>
{
    private readonly IDbContextFactory<FacilitationDbContext> _contextFactory;
    private readonly IHubContext<MeetingHub> _hubContext;

    public RaiseConcernHandler(IDbContextFactory<FacilitationDbContext> contextFactory, IHubContext<MeetingHub> hubContext)
    {
        _contextFactory = contextFactory;
        _hubContext = hubContext;
    }

    public async ValueTask<Guid> Handle(RaiseConcernCommand request, CancellationToken cancellationToken)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        
        var meeting = await context.Meetings
            .FirstOrDefaultAsync(m => m.Id == request.MeetingId, cancellationToken);

        if (meeting == null)
            throw new InvalidOperationException("Meeting not found");

        // Check resource limit: max 50 concerns per meeting
        var concernCount = await context.Concerns
            .CountAsync(c => c.MeetingId == request.MeetingId, cancellationToken);
        
        if (concernCount >= 50)
            throw new InvalidOperationException("Meeting has reached the maximum limit of 50 concerns.");

        var concern = new Concern
        {
            Id = Guid.NewGuid(),
            MeetingId = request.MeetingId,
            SessionId = request.SessionId,
            ConcernType = request.ConcernType,
            CustomText = string.IsNullOrWhiteSpace(request.CustomText) ? null : HtmlEncoder.Default.Encode(request.CustomText),
            CreatedAt = DateTime.UtcNow,
            IsDismissed = false,
            IsAcknowledged = false,
            IsWithdrawn = false
        };

        context.Concerns.Add(concern);
        await context.SaveChangesAsync(cancellationToken);
        
        // Notify all clients
        await _hubContext.Clients.Group(request.MeetingId.ToString())
            .SendAsync("MeetingUpdated", "concern_raised", cancellationToken);

        return concern.Id;
    }
}
