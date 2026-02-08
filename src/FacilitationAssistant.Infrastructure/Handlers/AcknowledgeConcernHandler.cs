using FacilitationAssistant.Core.Commands;
using FacilitationAssistant.Infrastructure.Data;
using Mediator;

namespace FacilitationAssistant.Infrastructure.Handlers;

public class AcknowledgeConcernHandler : ICommandHandler<AcknowledgeConcernCommand>
{
    private readonly FacilitationDbContext _context;

    public AcknowledgeConcernHandler(FacilitationDbContext context)
    {
        _context = context;
    }

    public async ValueTask<Unit> Handle(AcknowledgeConcernCommand request, CancellationToken cancellationToken)
    {
        var concern = await _context.Concerns.FindAsync(new object[] { request.ConcernId }, cancellationToken);
        if (concern == null)
        {
            throw new InvalidOperationException("Concern not found");
        }

        concern.IsAcknowledged = true;
        concern.AcknowledgedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
