using MediatR;

namespace FacilitationAssistant.Core.Commands;

public record CreateMeetingCommand(string Title) : IRequest<CreateMeetingResult>;

public record CreateMeetingResult(
    Guid MeetingId,
    string FacilitatorToken,
    string AttendeeToken
);
