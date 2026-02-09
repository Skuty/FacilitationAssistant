using FacilitationAssistant.Core.Commands;
using FacilitationAssistant.Core.Entities;
using FacilitationAssistant.Infrastructure.Data;
using Mediator;
using System.Text.Encodings.Web;

namespace FacilitationAssistant.Infrastructure.Handlers;

public class CreateMeetingHandler : IRequestHandler<CreateMeetingCommand, CreateMeetingResult>
{
    private readonly FacilitationDbContext _context;

    public CreateMeetingHandler(FacilitationDbContext context)
    {
        _context = context;
    }

    public async ValueTask<CreateMeetingResult> Handle(CreateMeetingCommand request, CancellationToken cancellationToken)
    {
        var facilitatorToken = GenerateToken();
        var attendeeToken = GenerateToken();

        var meeting = new Meeting
        {
            Id = Guid.NewGuid(),
            Title = string.IsNullOrWhiteSpace(request.Title) ? "New Meeting" : HtmlEncoder.Default.Encode(request.Title),
            FacilitatorToken = facilitatorToken,
            AttendeeToken = attendeeToken,
            CreatedAt = DateTime.UtcNow,
            Status = MeetingStatus.Setup
        };

        _context.Meetings.Add(meeting);
        await _context.SaveChangesAsync(cancellationToken);

        return new CreateMeetingResult(meeting.Id, facilitatorToken, attendeeToken);
    }

    private static string GenerateToken()
    {
        return Guid.NewGuid().ToString("N")[..20];
    }
}
