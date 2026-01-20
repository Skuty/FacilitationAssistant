using FacilitationAssistant.Core.Domain.Common;
using MediatR;

namespace FacilitationAssistant.Core.Application.Commands;

/// <summary>
/// Command to cast a vote on a poll.
/// </summary>
public record VoteCommand(
    Guid MeetingId,
    Guid PollId,
    Guid OptionId,
    string VoterSessionId
) : IRequest<Result>;
