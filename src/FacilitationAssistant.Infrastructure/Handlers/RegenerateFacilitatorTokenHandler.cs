using FacilitationAssistant.Core.Commands;
using FacilitationAssistant.Infrastructure.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FacilitationAssistant.Infrastructure.Handlers;

/// <summary>
/// Handles regenerating the facilitator token for a meeting.
/// </summary>
public class RegenerateFacilitatorTokenHandler : IRequestHandler<RegenerateFacilitatorTokenCommand, string>
{
    private readonly IDbContextFactory<FacilitationDbContext> _contextFactory;

    public RegenerateFacilitatorTokenHandler(IDbContextFactory<FacilitationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async ValueTask<string> Handle(RegenerateFacilitatorTokenCommand request, CancellationToken cancellationToken)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        
        var meeting = await context.Meetings
            .FirstOrDefaultAsync(m => m.FacilitatorToken == request.CurrentFacilitatorToken, cancellationToken);

        if (meeting == null)
        {
            throw new InvalidOperationException("Meeting not found or invalid facilitator token.");
        }

        // Generate new cryptographically secure token (32+ characters)
        var newToken = Guid.NewGuid().ToString("N");

        // Update the facilitator token
        meeting.FacilitatorToken = newToken;
        await context.SaveChangesAsync(cancellationToken);

        return newToken;
    }
}
