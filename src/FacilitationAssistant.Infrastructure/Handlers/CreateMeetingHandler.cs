using FacilitationAssistant.Core.Commands;
using FacilitationAssistant.Core.Entities;
using FacilitationAssistant.Infrastructure.Data;
using Mediator;
using Microsoft.Extensions.Logging;
using System.Text.Encodings.Web;

namespace FacilitationAssistant.Infrastructure.Handlers;

public class CreateMeetingHandler : IRequestHandler<CreateMeetingCommand, CreateMeetingResult>
{
    private readonly FacilitationDbContext _context;
    private readonly ILogger<CreateMeetingHandler> _logger;

    public CreateMeetingHandler(FacilitationDbContext context, ILogger<CreateMeetingHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async ValueTask<CreateMeetingResult> Handle(CreateMeetingCommand request, CancellationToken cancellationToken)
    {
        try
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

            _logger.LogInformation("Meeting {MeetingId} created with title: {Title}", meeting.Id, meeting.Title);

            return new CreateMeetingResult(meeting.Id, facilitatorToken, attendeeToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating meeting with title: {Title}", request.Title);
            throw;
        }
    }

    private static string GenerateToken()
    {
        return Guid.NewGuid().ToString("N")[..20];
    }
}
