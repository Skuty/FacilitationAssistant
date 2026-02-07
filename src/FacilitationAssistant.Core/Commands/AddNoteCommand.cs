using MediatR;

namespace FacilitationAssistant.Core.Commands;

public record AddNoteCommand(
    Guid MeetingId,
    string SessionId,
    string Content,
    bool IsPublic,
    Guid? StageId = null
) : IRequest<Guid>;
