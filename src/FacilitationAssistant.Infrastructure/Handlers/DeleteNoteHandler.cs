using FacilitationAssistant.Core.Commands;
using FacilitationAssistant.Infrastructure.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FacilitationAssistant.Infrastructure.Handlers;

public class DeleteNoteHandler : IRequestHandler<DeleteNoteCommand, bool>
{
    private readonly FacilitationDbContext _context;

    public DeleteNoteHandler(FacilitationDbContext context)
    {
        _context = context;
    }

    public async ValueTask<bool> Handle(DeleteNoteCommand request, CancellationToken cancellationToken)
    {
        var note = await _context.Notes
            .FirstOrDefaultAsync(n => n.Id == request.NoteId, cancellationToken);
        
        if (note == null)
            return false;

        // Only the note owner can delete it
        if (note.SessionId != request.SessionId)
            return false;

        _context.Notes.Remove(note);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
