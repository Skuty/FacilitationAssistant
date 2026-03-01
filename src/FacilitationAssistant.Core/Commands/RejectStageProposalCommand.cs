using Mediator;

namespace FacilitationAssistant.Core.Commands;

public record RejectStageProposalCommand(
    Guid MeetingId,
    Guid ProposalId,
    string? RejectionReason
) : IRequest<bool>;
