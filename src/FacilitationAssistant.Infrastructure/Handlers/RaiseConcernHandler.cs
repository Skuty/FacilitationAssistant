using FacilitationAssistant.Core.Commands;
using FacilitationAssistant.Core.Entities;
using FacilitationAssistant.Infrastructure.Data;
using Mediator;

namespace FacilitationAssistant.Infrastructure.Handlers;

public class RaiseConcernHandler : IRequestHandler<RaiseConcernCommand, Guid>
{
    private readonly FacilitationDbContext _context;

    public RaiseConcernHandler(FacilitationDbContext context)
    {
        _context = context;
    }

    public async ValueTask<Guid> Handle(RaiseConcernCommand request, CancellationToken cancellationToken)
    {
        var concern = new Concern
        {
            Id = Guid.NewGuid(),
            MeetingId = request.MeetingId,
            SessionId = request.SessionId,
            ConcernType = request.ConcernType,
            CustomText = request.CustomText,
            CreatedAt = DateTime.UtcNow,
            IsDismissed = false,
            IsAcknowledged = false,
            IsWithdrawn = false
        };

        _context.Concerns.Add(concern);
        await _context.SaveChangesAsync(cancellationToken);

        return concern.Id;
    }
}
