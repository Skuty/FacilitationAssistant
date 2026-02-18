using FacilitationAssistant.Core.Commands;
using FacilitationAssistant.Infrastructure.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FacilitationAssistant.Infrastructure.Handlers;

/// <summary>
/// Handles deleting a note from a meeting.
/// </summary>
public class DeleteNoteHandler : IRequestHandler<DeleteNoteCommand, bool>
{
    private readonly IDbContextFactory<FacilitationDbContext> _contextFactory;

    public DeleteNoteHandler(IDbContextFactory<FacilitationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async ValueTask<bool> Handle(DeleteNoteCommand request, CancellationToken cancellationToken)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        
        var note = await context.Notes
            .FirstOrDefaultAsync(n => n.Id == request.NoteId, cancellationToken);
        
        if (note == null)
            return false;

        // Only the note owner can delete it
        if (note.SessionId != request.SessionId)
            return false;

        context.Notes.Remove(note);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
