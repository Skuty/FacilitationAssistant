using FacilitationAssistant.Core.Domain.Common;
using MediatR;

namespace FacilitationAssistant.Core.Application.Commands;

/// <summary>
/// Command to add a note.
/// </summary>
public record AddNoteCommand(
    Guid MeetingId,
    string Content,
    string OwnerId,
    string OwnerName,
    bool IsPrivate,
    Guid? LinkedStageId = null
) : IRequest<Result<Guid>>;
