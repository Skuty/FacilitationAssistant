using FacilitationAssistant.Core.Entities;
using MediatR;

namespace FacilitationAssistant.Core.Queries;

public record GetMeetingByIdQuery(Guid MeetingId) : IRequest<Meeting?>;
