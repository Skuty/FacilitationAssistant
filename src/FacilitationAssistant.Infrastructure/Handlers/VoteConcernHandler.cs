using FacilitationAssistant.Core.Commands;
using FacilitationAssistant.Core.Entities;
using FacilitationAssistant.Infrastructure.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FacilitationAssistant.Infrastructure.Handlers;

public class VoteConcernHandler : ICommandHandler<VoteConcernCommand>
{
    private readonly FacilitationDbContext _context;

    public VoteConcernHandler(FacilitationDbContext context)
    {
        _context = context;
    }

    public async ValueTask<Unit> Handle(VoteConcernCommand request, CancellationToken cancellationToken)
    {
        var concern = await _context.Concerns
            .FirstOrDefaultAsync(c => c.Id == request.ConcernId, cancellationToken);
            
        if (concern == null)
            throw new InvalidOperationException("Concern not found");

        // Check if the user has already voted on this concern
        var existingVote = await _context.ConcernVotes
            .FirstOrDefaultAsync(v => v.ConcernId == request.ConcernId && v.SessionId == request.SessionId, cancellationToken);

        if (existingVote != null)
        {
            // If same vote type, remove the vote (toggle off)
            if (existingVote.VoteType == request.VoteType)
            {
                _context.ConcernVotes.Remove(existingVote);
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
            _context.ConcernVotes.Add(vote);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
