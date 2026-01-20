using FacilitationAssistant.Core.Domain.Common;
using FacilitationAssistant.Core.Domain.Enums;
using MediatR;

namespace FacilitationAssistant.Core.Application.Commands;

/// <summary>
/// Command to create a poll.
/// </summary>
public record CreatePollCommand(
    Guid MeetingId,
    string Question,
    Guid? StageId,
    bool AllowMultipleVotes,
    bool ShowResultsBeforeClose,
    List<string> Options
) : IRequest<Result<Guid>>;
