using FacilitationAssistant.Core.Commands;
using FacilitationAssistant.Infrastructure.Data;
using FacilitationAssistant.Infrastructure.Hubs;
using Mediator;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace FacilitationAssistant.Infrastructure.Handlers;

/// <summary>
/// Handles deleting a note from a meeting.
/// </summary>
public class DeleteNoteHandler : IRequestHandler<DeleteNoteCommand, bool>
{
    private readonly IDbContextFactory<FacilitationDbContext> _contextFactory;
    private readonly IHubContext<MeetingHub> _hubContext;

    public DeleteNoteHandler(
        IDbContextFactory<FacilitationDbContext> contextFactory,
        IHubContext<MeetingHub> hubContext)
    {
        _contextFactory = contextFactory;
        _hubContext = hubContext;
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

        var meetingId = note.MeetingId;
        
        context.Notes.Remove(note);
        await context.SaveChangesAsync(cancellationToken);
        
        // Notify all clients in the meeting group
        await _hubContext.Clients.Group(meetingId.ToString())
            .SendAsync("MeetingUpdated", "note_deleted", cancellationToken);
        
        return true;
    }
}
