using MediatR;

namespace FacilitationAssistant.Core.Application.Queries;

/// <summary>
/// Query to get meeting state for public consumption.
/// </summary>
public record GetMeetingStateQuery(Guid MeetingId) : IRequest<DTOs.MeetingStateDto?>;
