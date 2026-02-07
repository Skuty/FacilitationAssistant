using FacilitationAssistant.Core.Commands;
using FacilitationAssistant.Core.Entities;
using FacilitationAssistant.Infrastructure.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FacilitationAssistant.Infrastructure.Handlers;

public class EndMeetingHandler : IRequestHandler<EndMeetingCommand, Unit>
{
    private readonly FacilitationDbContext _context;

    public EndMeetingHandler(FacilitationDbContext context)
    {
        _context = context;
    }

    public async ValueTask<Unit> Handle(EndMeetingCommand request, CancellationToken cancellationToken)
    {
        var meeting = await _context.Meetings
            .Include(m => m.Stages)
            .FirstOrDefaultAsync(m => m.Id == request.MeetingId, cancellationToken);

        if (meeting == null)
            throw new InvalidOperationException("Meeting not found");

        if (meeting.Status == MeetingStatus.Ended)
            return Unit.Value; // Already ended

        // End any active stage
        var activeStage = meeting.Stages.FirstOrDefault(s => s.Status == StageStatus.Active);
        if (activeStage != null)
        {
            activeStage.Status = StageStatus.Completed;
            activeStage.CompletedAt = DateTime.UtcNow;
            activeStage.ActualDurationSeconds = (int)(activeStage.CompletedAt.Value - activeStage.StartedAt!.Value).TotalSeconds;
        }

        // Update meeting status
        meeting.Status = MeetingStatus.Ended;
        meeting.EndedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
