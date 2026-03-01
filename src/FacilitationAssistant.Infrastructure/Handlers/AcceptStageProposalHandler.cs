using FacilitationAssistant.Core.Commands;
using FacilitationAssistant.Core.Entities;
using FacilitationAssistant.Infrastructure.Data;
using FacilitationAssistant.Infrastructure.Hubs;
using Mediator;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace FacilitationAssistant.Infrastructure.Handlers;

/// <summary>
/// Handles a facilitator accepting a stage proposal: creates the AgendaStage and marks proposal Accepted.
/// </summary>
public class AcceptStageProposalHandler : IRequestHandler<AcceptStageProposalCommand, Guid>
{
    private readonly IDbContextFactory<FacilitationDbContext> _contextFactory;
    private readonly IHubContext<MeetingHub> _hubContext;

    public AcceptStageProposalHandler(
        IDbContextFactory<FacilitationDbContext> contextFactory,
        IHubContext<MeetingHub> hubContext)
    {
        _contextFactory = contextFactory;
        _hubContext = hubContext;
    }

    public async ValueTask<Guid> Handle(AcceptStageProposalCommand request, CancellationToken cancellationToken)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var proposal = await context.StageProposals
            .FirstOrDefaultAsync(p => p.Id == request.ProposalId && p.MeetingId == request.MeetingId, cancellationToken);

        if (proposal == null)
            throw new InvalidOperationException("Proposal not found");

        if (proposal.Status != StageProposalStatus.Pending)
            throw new InvalidOperationException("Proposal has already been reviewed");

        // Determine the next OrderIndex for the new stage
        var maxOrderIndex = await context.AgendaStages
            .Where(s => s.MeetingId == request.MeetingId)
            .Select(s => (int?)s.OrderIndex)
            .MaxAsync(cancellationToken);

        var nextOrderIndex = (maxOrderIndex ?? -1) + 1;

        var stage = new AgendaStage
        {
            Id = Guid.NewGuid(),
            MeetingId = request.MeetingId,
            Name = proposal.Name,
            Description = proposal.Description,
            PlannedDurationMinutes = proposal.PlannedDurationMinutes,
            OrderIndex = nextOrderIndex,
            Status = StageStatus.NotStarted
        };

        context.AgendaStages.Add(stage);

        proposal.Status = StageProposalStatus.Accepted;
        proposal.ReviewedAt = DateTime.UtcNow;
        proposal.AcceptedStageId = stage.Id;

        await context.SaveChangesAsync(cancellationToken);

        await _hubContext.Clients.Group(request.MeetingId.ToString())
            .SendAsync("MeetingUpdated", "stage_proposal_accepted", cancellationToken);

        return stage.Id;
    }
}
