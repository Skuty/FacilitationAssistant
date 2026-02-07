using FacilitationAssistant.Core.Entities;
using Mediator;

namespace FacilitationAssistant.Core.Queries;

public record GetMeetingByIdQuery(Guid MeetingId) : IRequest<Meeting?>;
