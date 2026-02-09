using FacilitationAssistant.Core.Commands;
using FacilitationAssistant.Core.Entities;
using FacilitationAssistant.Infrastructure.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;
using System.Text.Encodings.Web;

namespace FacilitationAssistant.Infrastructure.Handlers;

public class AddNoteHandler : IRequestHandler<AddNoteCommand, Guid>
{
    private readonly FacilitationDbContext _context;

    public AddNoteHandler(FacilitationDbContext context)
    {
        _context = context;
    }

    public async ValueTask<Guid> Handle(AddNoteCommand request, CancellationToken cancellationToken)
    {
        // Check resource limit: max 100 notes per meeting
        var noteCount = await _context.Notes
            .CountAsync(n => n.MeetingId == request.MeetingId, cancellationToken);
        
        if (noteCount >= 100)
            throw new InvalidOperationException("Meeting has reached the maximum limit of 100 notes.");

        var note = new Note
        {
            Id = Guid.NewGuid(),
            MeetingId = request.MeetingId,
            SessionId = request.SessionId,
            Content = HtmlEncoder.Default.Encode(request.Content),
            IsPublic = request.IsPublic,
            StageId = request.StageId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Notes.Add(note);
        await _context.SaveChangesAsync(cancellationToken);

        return note.Id;
    }
}
