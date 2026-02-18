using FacilitationAssistant.Core.Commands;
using FacilitationAssistant.Infrastructure.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FacilitationAssistant.Infrastructure.Handlers;

/// <summary>
/// Handles withdrawing a concern that was previously raised.
/// </summary>
public class WithdrawConcernHandler : ICommandHandler<WithdrawConcernCommand>
{
    private readonly IDbContextFactory<FacilitationDbContext> _contextFactory;

    public WithdrawConcernHandler(IDbContextFactory<FacilitationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
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
        return Unit.Value;
    }
}
