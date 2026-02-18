using FacilitationAssistant.Core.Commands;
using FacilitationAssistant.Core.Entities;
using FacilitationAssistant.Infrastructure.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;
using System.Text.Encodings.Web;

namespace FacilitationAssistant.Infrastructure.Handlers;

public class AddAgendaStageHandler : IRequestHandler<AddAgendaStageCommand, Guid>
{
    private readonly FacilitationDbContext _context;

    public AddAgendaStageHandler(FacilitationDbContext context)
    {
        _context = context;
    }

    public async ValueTask<Guid> Handle(AddAgendaStageCommand request, CancellationToken cancellationToken)
    {
        var meeting = await _context.Meetings
            .FirstOrDefaultAsync(m => m.Id == request.MeetingId, cancellationToken);

        if (meeting == null)
            throw new InvalidOperationException("Meeting not found");

        if (meeting.Status != MeetingStatus.Setup)
            throw new InvalidOperationException("Cannot add stages to a started meeting");

        var stage = new AgendaStage
        {
            Id = Guid.NewGuid(),
            MeetingId = request.MeetingId,
            Name = HtmlEncoder.Default.Encode(request.Name),
            Description = string.IsNullOrWhiteSpace(request.Description) ? string.Empty : HtmlEncoder.Default.Encode(request.Description),
            PlannedDurationMinutes = request.PlannedDurationMinutes,
            OrderIndex = request.OrderIndex,
            Status = StageStatus.NotStarted
        };

        _context.AgendaStages.Add(stage);
        await _context.SaveChangesAsync(cancellationToken);

        return stage.Id;
    }
}
