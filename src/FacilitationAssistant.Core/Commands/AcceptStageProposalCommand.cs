using Mediator;

namespace FacilitationAssistant.Core.Commands;

public record AcceptStageProposalCommand(
    Guid MeetingId,
    Guid ProposalId
) : IRequest<Guid>;
