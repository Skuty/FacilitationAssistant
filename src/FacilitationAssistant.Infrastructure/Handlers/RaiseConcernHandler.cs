using FacilitationAssistant.Core.Commands;
using FacilitationAssistant.Core.Entities;
using FacilitationAssistant.Infrastructure.Data;
using MediatR;

namespace FacilitationAssistant.Infrastructure.Handlers;

public class RaiseConcernHandler : IRequestHandler<RaiseConcernCommand, Guid>
{
    private readonly FacilitationDbContext _context;

    public RaiseConcernHandler(FacilitationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(RaiseConcernCommand request, CancellationToken cancellationToken)
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
            VotesUp = 0,
            VotesDown = 0
        };

        _context.Concerns.Add(concern);
        await _context.SaveChangesAsync(cancellationToken);

        return concern.Id;
    }
}
