using FacilitationAssistant.Core.Entities;
using Mediator;

namespace FacilitationAssistant.Core.Commands;

public record CreateMessageCommand(
    Guid MeetingId,
    string Text,
    MessageType Type,
    MessageResponseType? ResponseType,
    List<string>? PredefinedOptions
) : IRequest<Guid>;
