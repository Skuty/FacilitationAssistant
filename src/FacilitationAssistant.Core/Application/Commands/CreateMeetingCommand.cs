using FacilitationAssistant.Core.Domain.Common;
using MediatR;

namespace FacilitationAssistant.Core.Application.Commands;

/// <summary>
/// Command to create a new meeting.
/// </summary>
public record CreateMeetingCommand(string Title) : IRequest<Result<Guid>>;
