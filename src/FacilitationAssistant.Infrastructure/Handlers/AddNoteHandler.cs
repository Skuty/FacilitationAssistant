using FacilitationAssistant.Core.Commands;
using FacilitationAssistant.Core.Entities;
using FacilitationAssistant.Infrastructure.Data;
using FacilitationAssistant.Infrastructure.Hubs;
using Mediator;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Text.Encodings.Web;

namespace FacilitationAssistant.Infrastructure.Handlers;

/// <summary>
/// Handles adding a new note to a meeting.
/// </summary>
public class AddNoteHandler : IRequestHandler<AddNoteCommand, Guid>
{
    private readonly IDbContextFactory<FacilitationDbContext> _contextFactory;
    private readonly IHubContext<MeetingHub> _hubContext;

    public AddNoteHandler(IDbContextFactory<FacilitationDbContext> contextFactory, IHubContext<MeetingHub> hubContext)
    {
        _contextFactory = contextFactory;
        _hubContext = hubContext;
    }

    public async ValueTask<Guid> Handle(AddNoteCommand request, CancellationToken cancellationToken)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        
        var meeting = await context.Meetings
            .FirstOrDefaultAsync(m => m.Id == request.MeetingId, cancellationToken);

        if (meeting == null)
            throw new InvalidOperationException("Meeting not found");

        // Check resource limit: max 100 notes per meeting
        var noteCount = await context.Notes
            .CountAsync(n => n.MeetingId == request.MeetingId, cancellationToken);
        
        if (noteCount >= 100)
            throw new InvalidOperationException("Meeting has reached the maximum limit of 100 notes.");

        if (request.AuthorName != null && request.AuthorName.Length > 50)
            throw new ArgumentException("Author name must not exceed 50 characters.");

        var note = new Note
        {
            Id = Guid.NewGuid(),
            MeetingId = request.MeetingId,
            SessionId = request.SessionId,
            AuthorName = string.IsNullOrWhiteSpace(request.AuthorName) ? null : HtmlEncoder.Default.Encode(request.AuthorName),
            Content = HtmlEncoder.Default.Encode(request.Content),
            IsPublic = request.IsPublic,
            StageId = request.StageId,
            CreatedAt = DateTime.UtcNow
        };

        context.Notes.Add(note);
        await context.SaveChangesAsync(cancellationToken);
        
        // Notify all clients
        await _hubContext.Clients.Group(request.MeetingId.ToString())
            .SendAsync("MeetingUpdated", "note_added", cancellationToken);

        return note.Id;
    }
}
