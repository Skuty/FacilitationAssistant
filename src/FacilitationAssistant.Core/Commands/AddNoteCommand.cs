using Mediator;

namespace FacilitationAssistant.Core.Commands;

public record AddNoteCommand(
    Guid MeetingId,
    string SessionId,
    string AuthorName,
    string Content,
    bool IsPublic,
    Guid? StageId = null
) : IRequest<Guid>;
