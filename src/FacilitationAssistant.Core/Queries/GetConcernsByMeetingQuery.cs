using Mediator;
using FacilitationAssistant.Core.Entities;

namespace FacilitationAssistant.Core.Queries;

public record GetConcernsByMeetingQuery(
    Guid MeetingId
) : IQuery<IReadOnlyList<Concern>>;
