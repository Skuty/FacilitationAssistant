using FacilitationAssistant.Core.Domain.Common;
using MediatR;

namespace FacilitationAssistant.Core.Application.Commands;

/// <summary>
/// Command to start a specific stage.
/// </summary>
public record StartStageCommand(Guid MeetingId, Guid StageId) : IRequest<Result>;
