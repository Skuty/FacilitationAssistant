using FacilitationAssistant.Core.Domain.Common;
using MediatR;

namespace FacilitationAssistant.Core.Application.Commands;

/// <summary>
/// Command to add an agenda stage to a meeting.
/// </summary>
public record AddAgendaStageCommand(
    Guid MeetingId,
    string Title,
    string? Description,
    int DurationMinutes,
    int SortOrder
) : IRequest<Result>;
