using FacilitationAssistant.Core.Commands;
using FacilitationAssistant.Core.Entities;
using FacilitationAssistant.Infrastructure.Data;
using FacilitationAssistant.Infrastructure.Hubs;
using Mediator;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace FacilitationAssistant.Infrastructure.Handlers;

/// <summary>
/// Handles a facilitator rejecting a stage proposal.
/// </summary>
public class RejectStageProposalHandler : IRequestHandler<RejectStageProposalCommand, bool>
{
    private readonly IDbContextFactory<FacilitationDbContext> _contextFactory;
    private readonly IHubContext<MeetingHub> _hubContext;

    public RejectStageProposalHandler(
        IDbContextFactory<FacilitationDbContext> contextFactory,
        IHubContext<MeetingHub> hubContext)
    {
        _contextFactory = contextFactory;
        _hubContext = hubContext;
    }

    public async ValueTask<bool> Handle(RejectStageProposalCommand request, CancellationToken cancellationToken)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var proposal = await context.StageProposals
            .FirstOrDefaultAsync(p => p.Id == request.ProposalId && p.MeetingId == request.MeetingId, cancellationToken);

        if (proposal == null)
            throw new InvalidOperationException("Proposal not found");

        if (proposal.Status != StageProposalStatus.Pending)
            throw new InvalidOperationException("Proposal has already been reviewed");

        proposal.Status = StageProposalStatus.Rejected;
        proposal.ReviewedAt = DateTime.UtcNow;
        proposal.RejectionReason = request.RejectionReason?.Trim();

        await context.SaveChangesAsync(cancellationToken);

        await _hubContext.Clients.Group(request.MeetingId.ToString())
            .SendAsync("MeetingUpdated", "stage_proposal_rejected", cancellationToken);

        return true;
    }
}
