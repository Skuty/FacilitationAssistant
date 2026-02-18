using FacilitationAssistant.Core.Commands;
using FacilitationAssistant.Core.Entities;
using FacilitationAssistant.Infrastructure.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;
using System.Text.Encodings.Web;

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
        var meeting = await _context.Meetings
            .FirstOrDefaultAsync(m => m.Id == request.MeetingId, cancellationToken);

        if (meeting == null)
            throw new InvalidOperationException("Meeting not found");

        // Check resource limit: max 50 concerns per meeting
        var concernCount = await _context.Concerns
            .CountAsync(c => c.MeetingId == request.MeetingId, cancellationToken);
        
        if (concernCount >= 50)
            throw new InvalidOperationException("Meeting has reached the maximum limit of 50 concerns.");

        var concern = new Concern
        {
            Id = Guid.NewGuid(),
            MeetingId = request.MeetingId,
            SessionId = request.SessionId,
            ConcernType = request.ConcernType,
            CustomText = string.IsNullOrWhiteSpace(request.CustomText) ? null : HtmlEncoder.Default.Encode(request.CustomText),
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
