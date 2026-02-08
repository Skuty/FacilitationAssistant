using Mediator;

namespace FacilitationAssistant.Core.Commands;

public record UpdateNoteCommand(
    Guid NoteId,
    string SessionId,
    string? Content = null,
    bool? IsPublic = null
) : IRequest<bool>;
