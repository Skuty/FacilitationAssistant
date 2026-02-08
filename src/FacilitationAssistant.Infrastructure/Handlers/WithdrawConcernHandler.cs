using FacilitationAssistant.Core.Commands;
using FacilitationAssistant.Infrastructure.Data;
using Mediator;

namespace FacilitationAssistant.Infrastructure.Handlers;

public class WithdrawConcernHandler : ICommandHandler<WithdrawConcernCommand>
{
    private readonly FacilitationDbContext _context;

    public WithdrawConcernHandler(FacilitationDbContext context)
    {
        _context = context;
    }

    public async ValueTask<Unit> Handle(WithdrawConcernCommand request, CancellationToken cancellationToken)
    {
        var concern = await _context.Concerns.FindAsync(new object[] { request.ConcernId }, cancellationToken);
        if (concern == null)
        {
            throw new InvalidOperationException("Concern not found");
        }

        // Only the person who raised the concern can withdraw it
        if (concern.SessionId != request.SessionId)
        {
            throw new InvalidOperationException("You can only withdraw your own concerns");
        }

        concern.IsWithdrawn = true;
        concern.WithdrawnAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
