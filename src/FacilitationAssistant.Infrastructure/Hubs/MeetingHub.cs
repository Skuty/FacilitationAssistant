using Microsoft.AspNetCore.SignalR;

namespace FacilitationAssistant.Infrastructure.Hubs;

/// <summary>
/// SignalR hub for real-time meeting updates
/// </summary>
public class MeetingHub : Hub
{
    public async Task JoinMeeting(string meetingId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, meetingId);
    }

    public async Task LeaveMeeting(string meetingId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, meetingId);
    }

    public async Task NotifyMeetingUpdate(string meetingId, string updateType)
    {
        await Clients.Group(meetingId).SendAsync("MeetingUpdated", updateType);
    }
}
