using FacilitationAssistant.Core.Entities;
using Mediator;

namespace FacilitationAssistant.Core.Commands;

public record VoteConcernCommand(
    Guid ConcernId,
    string SessionId,
    VoteType VoteType
) : ICommand<Unit>;
