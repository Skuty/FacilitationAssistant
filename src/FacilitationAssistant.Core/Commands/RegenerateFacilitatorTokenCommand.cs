using Mediator;

namespace FacilitationAssistant.Core.Commands;

public record RegenerateFacilitatorTokenCommand(string CurrentFacilitatorToken) : IRequest<string>;
