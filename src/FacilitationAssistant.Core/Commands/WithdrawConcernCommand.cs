using Mediator;

namespace FacilitationAssistant.Core.Commands;

public record WithdrawConcernCommand(
    Guid ConcernId,
    string SessionId
) : ICommand<Unit>;
