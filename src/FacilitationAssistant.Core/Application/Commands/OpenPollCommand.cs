using FacilitationAssistant.Core.Domain.Common;
using MediatR;

namespace FacilitationAssistant.Core.Application.Commands;

/// <summary>
/// Command to open a poll for voting.
/// </summary>
public record OpenPollCommand(Guid MeetingId, Guid PollId) : IRequest<Result>;
