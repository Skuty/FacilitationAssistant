using FacilitationAssistant.Core.Commands;
using FacilitationAssistant.Infrastructure.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;
using System.Text.Encodings.Web;

namespace FacilitationAssistant.Infrastructure.Handlers;

public class UpdateNoteHandler : IRequestHandler<UpdateNoteCommand, bool>
{
    private readonly FacilitationDbContext _context;

    public UpdateNoteHandler(FacilitationDbContext context)
    {
        _context = context;
    }

    public async ValueTask<bool> Handle(UpdateNoteCommand request, CancellationToken cancellationToken)
    {
        var note = await _context.Notes
            .FirstOrDefaultAsync(n => n.Id == request.NoteId, cancellationToken);

        if (note == null)
            return false;

        // Only the note owner can update it
        if (note.SessionId != request.SessionId)
            return false;

        // Update content if provided
        if (request.Content != null)
        {
            note.Content = HtmlEncoder.Default.Encode(request.Content);
            note.UpdatedAt = DateTime.UtcNow;
        }

        // Update visibility if provided
        if (request.IsPublic.HasValue)
        {
            note.IsPublic = request.IsPublic.Value;
            note.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
