using FacilitationAssistant.Core.Commands;
using FacilitationAssistant.Core.Entities;
using FacilitationAssistant.Infrastructure.Data;
using FacilitationAssistant.Infrastructure.Hubs;
using Mediator;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Text.Encodings.Web;

namespace FacilitationAssistant.Infrastructure.Handlers;

/// <summary>
/// Handles creating a new message for attendees during a meeting.
/// </summary>
public class CreateMessageHandler : IRequestHandler<CreateMessageCommand, Guid>
{
    private readonly IDbContextFactory<FacilitationDbContext> _contextFactory;
    private readonly IHubContext<MeetingHub> _hubContext;

    public CreateMessageHandler(IDbContextFactory<FacilitationDbContext> contextFactory, IHubContext<MeetingHub> hubContext)
    {
        _contextFactory = contextFactory;
        _hubContext = hubContext;
    }

    public async ValueTask<Guid> Handle(CreateMessageCommand request, CancellationToken cancellationToken)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        
        var meeting = await context.Meetings
            .FirstOrDefaultAsync(m => m.Id == request.MeetingId, cancellationToken);

        if (meeting == null)
            throw new InvalidOperationException("Meeting not found");

        // Validate message text
        if (string.IsNullOrWhiteSpace(request.Text) || request.Text.Length > 1000)
            throw new ArgumentException("Message text must be between 1 and 1000 characters");

        // Validate question type has response type
        if (request.Type == MessageType.Question && request.ResponseType == null)
            throw new ArgumentException("Question messages must have a response type");

        // Validate predefined answers
        if (request.ResponseType == MessageResponseType.PredefinedAnswers)
        {
            if (request.PredefinedOptions == null || request.PredefinedOptions.Count < 2 || request.PredefinedOptions.Count > 6)
                throw new ArgumentException("Predefined answers must have between 2 and 6 options");

            if (request.PredefinedOptions.Any(o => string.IsNullOrWhiteSpace(o) || o.Length > 100))
                throw new ArgumentException("Option text must be between 1 and 100 characters");
        }

        var message = new Message
        {
            Id = Guid.NewGuid(),
            MeetingId = request.MeetingId,
            Text = HtmlEncoder.Default.Encode(request.Text),
            Type = request.Type,
            ResponseType = request.ResponseType,
            CreatedAt = DateTime.UtcNow,
            IsClosed = false
        };

        context.Messages.Add(message);
        
        // Add options for predefined answers
        if (request.PredefinedOptions != null && request.PredefinedOptions.Any())
        {
            for (int i = 0; i < request.PredefinedOptions.Count; i++)
            {
                var option = new MessageOption
                {
                    Id = Guid.NewGuid(),
                    MessageId = message.Id,
                    Text = HtmlEncoder.Default.Encode(request.PredefinedOptions[i]),
                    OrderIndex = i
                };
                context.MessageOptions.Add(option);
            }
        }

        await context.SaveChangesAsync(cancellationToken);
        
        // Notify all clients
        await _hubContext.Clients.Group(request.MeetingId.ToString())
            .SendAsync("MeetingUpdated", "message_sent", cancellationToken);
        
        return message.Id;
    }
}
