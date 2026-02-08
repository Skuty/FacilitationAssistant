using Mediator;

namespace FacilitationAssistant.Core.Commands;

public record DeleteNoteCommand(
    Guid NoteId,
    string SessionId
) : IRequest<bool>;
