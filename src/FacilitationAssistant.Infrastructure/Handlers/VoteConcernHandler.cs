using FacilitationAssistant.Core.Commands;
using FacilitationAssistant.Core.Entities;
using FacilitationAssistant.Infrastructure.Data;
using FacilitationAssistant.Infrastructure.Hubs;
using Mediator;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace FacilitationAssistant.Infrastructure.Handlers;

/// <summary>
/// Handles voting on a concern (upvote or downvote).
/// </summary>
public class VoteConcernHandler : ICommandHandler<VoteConcernCommand>
{
    private readonly IDbContextFactory<FacilitationDbContext> _contextFactory;
    private readonly IHubContext<MeetingHub> _hubContext;

    public VoteConcernHandler(IDbContextFactory<FacilitationDbContext> contextFactory, IHubContext<MeetingHub> hubContext)
    {
        _contextFactory = contextFactory;
        _hubContext = hubContext;
    }

    public async ValueTask<Unit> Handle(VoteConcernCommand request, CancellationToken cancellationToken)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        
        var concern = await context.Concerns
            .FirstOrDefaultAsync(c => c.Id == request.ConcernId, cancellationToken);
            
        if (concern == null)
            throw new InvalidOperationException("Concern not found");

        // Check if the user has already voted on this concern
        var existingVote = await context.ConcernVotes
            .FirstOrDefaultAsync(v => v.ConcernId == request.ConcernId && v.SessionId == request.SessionId, cancellationToken);

        if (existingVote != null)
        {
            // If same vote type, remove the vote (toggle off)
            if (existingVote.VoteType == request.VoteType)
            {
                context.ConcernVotes.Remove(existingVote);
            }
            else
            {
                // Update to new vote type (change vote)
                existingVote.VoteType = request.VoteType;
                existingVote.VotedAt = DateTime.UtcNow;
            }
        }
        else
        {
            // Create new vote
            var vote = new ConcernVote
            {
                Id = Guid.NewGuid(),
                ConcernId = request.ConcernId,
                SessionId = request.SessionId,
                VoteType = request.VoteType,
                VotedAt = DateTime.UtcNow
            };
            context.ConcernVotes.Add(vote);
        }

        await context.SaveChangesAsync(cancellationToken);
        
        // Notify all clients
        await _hubContext.Clients.Group(concern.MeetingId.ToString())
            .SendAsync("MeetingUpdated", "concern_voted", cancellationToken);
        
        return Unit.Value;
    }
}
