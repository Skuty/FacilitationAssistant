using FacilitationAssistant.Core.Commands;
using FacilitationAssistant.Infrastructure.Data;
using FacilitationAssistant.Infrastructure.Hubs;
using Mediator;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Text.Encodings.Web;

namespace FacilitationAssistant.Infrastructure.Handlers;

/// <summary>
/// Handles updating an existing note.
/// </summary>
public class UpdateNoteHandler : IRequestHandler<UpdateNoteCommand, bool>
{
    private readonly IDbContextFactory<FacilitationDbContext> _contextFactory;
    private readonly IHubContext<MeetingHub> _hubContext;

    public UpdateNoteHandler(
        IDbContextFactory<FacilitationDbContext> contextFactory,
        IHubContext<MeetingHub> hubContext)
    {
        _contextFactory = contextFactory;
        _hubContext = hubContext;
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
        
        // Notify all clients in the meeting group
        await _hubContext.Clients.Group(note.MeetingId.ToString())
            .SendAsync("MeetingUpdated", "note_updated", cancellationToken);
        
        return true;
    }
}
