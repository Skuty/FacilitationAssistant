using FacilitationAssistant.Core.Entities;
using FacilitationAssistant.Infrastructure.Data;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;

namespace FacilitationAssistant.Infrastructure.Hubs;

/// <summary>
/// SignalR hub for real-time meeting updates
/// </summary>
public class MeetingHub : Hub
{
    private readonly IDbContextFactory<FacilitationDbContext> _contextFactory;

    // Tracks connectionId → (meetingId, sessionId) so OnDisconnectedAsync can clean up
    private static readonly ConcurrentDictionary<string, (string MeetingId, string SessionId)> _connectionMap = new();

    public MeetingHub(IDbContextFactory<FacilitationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    /// <summary>
    /// Called by attendees when they join a meeting. Creates or reconnects an AttendeeSession
    /// in the database and notifies all group members (including the facilitator) so the
    /// participant list is refreshed in real-time.
    /// </summary>
    public async Task JoinMeeting(string meetingId, string? sessionId = null)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, meetingId);

        if (!string.IsNullOrEmpty(sessionId))
        {
            _connectionMap[Context.ConnectionId] = (meetingId, sessionId);

            if (Guid.TryParse(meetingId, out var meetingGuid))
            {
                await using var context = await _contextFactory.CreateDbContextAsync();

                var existing = await context.AttendeeSessions
                    .FirstOrDefaultAsync(s => s.MeetingId == meetingGuid && s.SessionId == sessionId);

                if (existing == null)
                {
                    context.AttendeeSessions.Add(new AttendeeSession
                    {
                        Id = Guid.NewGuid(),
                        MeetingId = meetingGuid,
                        SessionId = sessionId,
                        JoinedAt = DateTime.UtcNow,
                        LastSeenAt = DateTime.UtcNow,
                        IsConnected = true
                    });
                }
                else
                {
                    existing.IsConnected = true;
                    existing.LastSeenAt = DateTime.UtcNow;
                }

                await context.SaveChangesAsync();
            }

            // Notify all clients in the group (facilitator reloads participant list)
            await Clients.Group(meetingId).SendAsync("MeetingUpdated", "attendee_joined");
        }
    }

    /// <summary>
    /// Updates the display name for an attendee session after the user enters their name.
    /// </summary>
    public async Task UpdateDisplayName(string meetingId, string sessionId, string displayName)
    {
        if (!Guid.TryParse(meetingId, out var meetingGuid))
            return;

        await using var context = await _contextFactory.CreateDbContextAsync();

        var session = await context.AttendeeSessions
            .FirstOrDefaultAsync(s => s.MeetingId == meetingGuid && s.SessionId == sessionId);

        if (session != null)
        {
            session.DisplayName = displayName;
            session.LastSeenAt = DateTime.UtcNow;
            await context.SaveChangesAsync();
        }

        await Clients.Group(meetingId).SendAsync("MeetingUpdated", "attendee_updated");
    }

    public async Task LeaveMeeting(string meetingId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, meetingId);
        _connectionMap.TryRemove(Context.ConnectionId, out _);
    }

    public async Task NotifyMeetingUpdate(string meetingId, string updateType)
    {
        await Clients.Group(meetingId).SendAsync("MeetingUpdated", updateType);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (_connectionMap.TryRemove(Context.ConnectionId, out var info) &&
            Guid.TryParse(info.MeetingId, out var meetingGuid))
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var session = await context.AttendeeSessions
                .FirstOrDefaultAsync(s => s.MeetingId == meetingGuid && s.SessionId == info.SessionId);

            if (session != null)
            {
                session.IsConnected = false;
                session.LastSeenAt = DateTime.UtcNow;
                await context.SaveChangesAsync();
            }

            await Clients.Group(info.MeetingId).SendAsync("MeetingUpdated", "attendee_left");
        }

        await base.OnDisconnectedAsync(exception);
    }
}
