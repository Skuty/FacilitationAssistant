using FacilitationAssistant.Core.Commands;
using FacilitationAssistant.Infrastructure.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FacilitationAssistant.Infrastructure.Handlers;

public class RegenerateFacilitatorTokenHandler : IRequestHandler<RegenerateFacilitatorTokenCommand, string>
{
    private readonly FacilitationDbContext _context;

    public RegenerateFacilitatorTokenHandler(FacilitationDbContext context)
    {
        _context = context;
    }

    public async ValueTask<string> Handle(RegenerateFacilitatorTokenCommand request, CancellationToken cancellationToken)
    {
        var meeting = await _context.Meetings
            .FirstOrDefaultAsync(m => m.FacilitatorToken == request.CurrentFacilitatorToken, cancellationToken);

        if (meeting == null)
        {
            throw new InvalidOperationException("Meeting not found or invalid facilitator token.");
        }

        // Generate new cryptographically secure token (32+ characters)
        var newToken = Guid.NewGuid().ToString("N");

        // Update the facilitator token
        meeting.FacilitatorToken = newToken;
        await _context.SaveChangesAsync(cancellationToken);

        return newToken;
    }
}
