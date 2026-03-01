using FacilitationAssistant.Core.Commands;
using FacilitationAssistant.Core.Entities;
using FacilitationAssistant.Infrastructure.Data;
using FacilitationAssistant.Infrastructure.Hubs;
using Mediator;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace FacilitationAssistant.Infrastructure.Handlers;

/// <summary>
/// Handles a participant proposing a new agenda stage during an active meeting.
/// </summary>
public class ProposeStageHandler : IRequestHandler<ProposeStageCommand, Guid>
{
    private readonly IDbContextFactory<FacilitationDbContext> _contextFactory;
    private readonly IHubContext<MeetingHub> _hubContext;

    public ProposeStageHandler(
        IDbContextFactory<FacilitationDbContext> contextFactory,
        IHubContext<MeetingHub> hubContext)
    {
        _contextFactory = contextFactory;
        _hubContext = hubContext;
    }

    public async ValueTask<Guid> Handle(ProposeStageCommand request, CancellationToken cancellationToken)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var meeting = await context.Meetings
            .FirstOrDefaultAsync(m => m.Id == request.MeetingId, cancellationToken);

        if (meeting == null)
            throw new InvalidOperationException("Meeting not found");

        if (meeting.Status != MeetingStatus.Active)
            throw new InvalidOperationException("Stage proposals can only be submitted during an active meeting");

        if (string.IsNullOrWhiteSpace(request.Name))
            throw new InvalidOperationException("Stage name is required");

        if (request.PlannedDurationMinutes < 1 || request.PlannedDurationMinutes > 240)
            throw new InvalidOperationException("Duration must be between 1 and 240 minutes");

        var proposal = new StageProposal
        {
            Id = Guid.NewGuid(),
            MeetingId = request.MeetingId,
            SessionId = request.SessionId,
            ProposerName = request.ProposerName,
            Name = request.Name.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            PlannedDurationMinutes = request.PlannedDurationMinutes,
            Status = StageProposalStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        context.StageProposals.Add(proposal);
        await context.SaveChangesAsync(cancellationToken);

        await _hubContext.Clients.Group(request.MeetingId.ToString())
            .SendAsync("MeetingUpdated", "stage_proposal_submitted", cancellationToken);

        return proposal.Id;
    }
}
