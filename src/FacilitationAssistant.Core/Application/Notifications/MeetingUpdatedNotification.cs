using MediatR;

namespace FacilitationAssistant.Core.Application.Notifications;

/// <summary>
/// Notification published when meeting state changes.
/// </summary>
public record MeetingUpdatedNotification(Guid MeetingId) : INotification;
