using FacilitationAssistant.Core.Entities;
using Mediator;

namespace FacilitationAssistant.Core.Queries;

public record GetStageProposalsBySessionQuery(Guid MeetingId, string SessionId) : IRequest<IEnumerable<StageProposal>>;
