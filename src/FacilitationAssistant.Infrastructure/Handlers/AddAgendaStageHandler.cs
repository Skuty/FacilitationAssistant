using FacilitationAssistant.Core.Commands;
using FacilitationAssistant.Core.Entities;
using FacilitationAssistant.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FacilitationAssistant.Infrastructure.Handlers;

public class AddAgendaStageHandler : IRequestHandler<AddAgendaStageCommand, Guid>
{
    private readonly FacilitationDbContext _context;

    public AddAgendaStageHandler(FacilitationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(AddAgendaStageCommand request, CancellationToken cancellationToken)
    {
        var meeting = await _context.Meetings
            .Include(m => m.Stages)
            .FirstOrDefaultAsync(m => m.Id == request.MeetingId, cancellationToken);

        if (meeting == null)
            throw new InvalidOperationException("Meeting not found");

        if (meeting.Status != MeetingStatus.Setup)
            throw new InvalidOperationException("Cannot add stages to a started meeting");

        var stage = new AgendaStage
        {
            Id = Guid.NewGuid(),
            MeetingId = request.MeetingId,
            Name = request.Name,
            Description = request.Description,
            PlannedDurationMinutes = request.PlannedDurationMinutes,
            OrderIndex = request.OrderIndex,
            Status = StageStatus.NotStarted
        };

        _context.AgendaStages.Add(stage);
        await _context.SaveChangesAsync(cancellationToken);

        return stage.Id;
    }
}
