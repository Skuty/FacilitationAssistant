using FacilitationAssistant.Core.Commands;
using FacilitationAssistant.Infrastructure.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;
using System.Text.Encodings.Web;

namespace FacilitationAssistant.Infrastructure.Handlers;

/// <summary>
/// Handles updating an existing note.
/// </summary>
public class UpdateNoteHandler : IRequestHandler<UpdateNoteCommand, bool>
{
    private readonly IDbContextFactory<FacilitationDbContext> _contextFactory;

    public UpdateNoteHandler(IDbContextFactory<FacilitationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async ValueTask<bool> Handle(UpdateNoteCommand request, CancellationToken cancellationToken)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        
        var note = await context.Notes
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

        await context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
