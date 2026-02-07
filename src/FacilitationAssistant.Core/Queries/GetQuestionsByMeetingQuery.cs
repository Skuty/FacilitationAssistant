using FacilitationAssistant.Core.Entities;
using MediatR;

namespace FacilitationAssistant.Core.Queries;

public record GetQuestionsByMeetingQuery(Guid MeetingId) : IRequest<List<Question>>;
