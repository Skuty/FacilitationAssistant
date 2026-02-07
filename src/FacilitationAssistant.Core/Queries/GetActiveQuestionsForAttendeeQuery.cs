using FacilitationAssistant.Core.Entities;
using MediatR;

namespace FacilitationAssistant.Core.Queries;

public record GetActiveQuestionsForAttendeeQuery(
    Guid MeetingId,
    string AttendeeSessionId
) : IRequest<List<Question>>;
