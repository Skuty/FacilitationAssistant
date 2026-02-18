using FacilitationAssistant.Core.Commands;
using FacilitationAssistant.Core.Entities;
using FacilitationAssistant.Infrastructure.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Encodings.Web;

namespace FacilitationAssistant.Infrastructure.Handlers;

/// <summary>
/// Creates a new meeting with facilitator and attendee tokens
/// </summary>
public class CreateMeetingHandler : IRequestHandler<CreateMeetingCommand, CreateMeetingResult>
{
    private readonly IDbContextFactory<FacilitationDbContext> _contextFactory;
    private readonly ILogger<CreateMeetingHandler> _logger;

    public CreateMeetingHandler(
        IDbContextFactory<FacilitationDbContext> contextFactory, 
        ILogger<CreateMeetingHandler> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async ValueTask<CreateMeetingResult> Handle(CreateMeetingCommand request, CancellationToken cancellationToken)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        
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

            context.Meetings.Add(meeting);
            await context.SaveChangesAsync(cancellationToken);

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
