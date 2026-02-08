using FacilitationAssistant.Core.Commands;
using FacilitationAssistant.Infrastructure.Data;
using Mediator;

namespace FacilitationAssistant.Infrastructure.Handlers;

public class RespondToConcernHandler : ICommandHandler<RespondToConcernCommand>
{
    private readonly FacilitationDbContext _context;

    public RespondToConcernHandler(FacilitationDbContext context)
    {
        _context = context;
    }

    public async ValueTask<Unit> Handle(RespondToConcernCommand request, CancellationToken cancellationToken)
    {
        var concern = await _context.Concerns.FindAsync(new object[] { request.ConcernId }, cancellationToken);
        if (concern == null)
        {
            throw new InvalidOperationException("Concern not found");
        }

        concern.ResponseText = request.ResponseText;
        concern.RespondedAt = DateTime.UtcNow;
        
        // Automatically acknowledge when responding
        if (!concern.IsAcknowledged)
        {
            concern.IsAcknowledged = true;
            concern.AcknowledgedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
