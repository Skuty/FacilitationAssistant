using FacilitationAssistant.Core.Domain.Common;
using FacilitationAssistant.Core.Domain.Enums;
using MediatR;

namespace FacilitationAssistant.Core.Application.Commands;

/// <summary>
/// Command to raise a concern.
/// </summary>
public record RaiseConcernCommand(
    Guid MeetingId,
    string Content,
    string RaisedBySessionId,
    string? RaisedByName,
    bool IsAnonymous,
    ConcernSeverity Severity
) : IRequest<Result<Guid>>;
