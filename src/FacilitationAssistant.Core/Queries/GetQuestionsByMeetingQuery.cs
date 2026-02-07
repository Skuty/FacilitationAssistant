using FacilitationAssistant.Core.Entities;
using Mediator;

namespace FacilitationAssistant.Core.Queries;

public record GetQuestionsByMeetingQuery(Guid MeetingId) : IRequest<List<Question>>;
