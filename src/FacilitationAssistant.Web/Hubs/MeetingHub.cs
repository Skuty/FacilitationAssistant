using FacilitationAssistant.Core.Application.DTOs;
using FacilitationAssistant.Core.Application.Notifications;
using FacilitationAssistant.Core.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace FacilitationAssistant.Web.Hubs;

/// <summary>
/// SignalR hub for real-time meeting synchronization.
/// </summary>
public class MeetingHub : Hub
{
    private readonly IMediator _mediator;

    public MeetingHub(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Join a meeting room to receive updates.
    /// </summary>
    public async Task JoinMeeting(Guid meetingId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, meetingId.ToString());
        
        var state = await _mediator.Send(new GetMeetingStateQuery(meetingId));
        if (state != null)
        {
            await Clients.Caller.SendAsync("ReceiveState", state);
        }
    }

    /// <summary>
    /// Leave a meeting room.
    /// </summary>
    public async Task LeaveMeeting(Guid meetingId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, meetingId.ToString());
    }
}

/// <summary>
/// Notification handler that broadcasts meeting updates via SignalR.
/// </summary>
public class MeetingUpdatedNotificationHandler : INotificationHandler<MeetingUpdatedNotification>
{
    private readonly IHubContext<MeetingHub> _hubContext;
    private readonly IMediator _mediator;

    public MeetingUpdatedNotificationHandler(IHubContext<MeetingHub> hubContext, IMediator mediator)
    {
        _hubContext = hubContext;
        _mediator = mediator;
    }

    public async Task Handle(MeetingUpdatedNotification notification, CancellationToken ct)
    {
        var state = await _mediator.Send(new GetMeetingStateQuery(notification.MeetingId), ct);
        if (state != null)
        {
            await _hubContext.Clients
                .Group(notification.MeetingId.ToString())
                .SendAsync("ReceiveState", state, ct);
        }
    }
}
