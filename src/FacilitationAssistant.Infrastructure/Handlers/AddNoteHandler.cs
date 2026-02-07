using FacilitationAssistant.Core.Commands;
using FacilitationAssistant.Core.Entities;
using FacilitationAssistant.Infrastructure.Data;
using MediatR;

namespace FacilitationAssistant.Infrastructure.Handlers;

public class AddNoteHandler : IRequestHandler<AddNoteCommand, Guid>
{
    private readonly FacilitationDbContext _context;

    public AddNoteHandler(FacilitationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(AddNoteCommand request, CancellationToken cancellationToken)
    {
        var note = new Note
        {
            Id = Guid.NewGuid(),
            MeetingId = request.MeetingId,
            SessionId = request.SessionId,
            Content = request.Content,
            IsPublic = request.IsPublic,
            StageId = request.StageId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Notes.Add(note);
        await _context.SaveChangesAsync(cancellationToken);

        return note.Id;
    }
}
