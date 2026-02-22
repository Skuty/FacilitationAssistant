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
/// Handles submitting a response to a message.
/// </summary>
public class RespondToMessageHandler : IRequestHandler<RespondToMessageCommand, Guid>
{
    private readonly IDbContextFactory<FacilitationDbContext> _contextFactory;
    private readonly IHubContext<MeetingHub> _hubContext;

    public RespondToMessageHandler(IDbContextFactory<FacilitationDbContext> contextFactory, IHubContext<MeetingHub> hubContext)
    {
        _contextFactory = contextFactory;
        _hubContext = hubContext;
    }

    public async ValueTask<Guid> Handle(RespondToMessageCommand request, CancellationToken cancellationToken)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        
        var message = await context.Messages
            .FirstOrDefaultAsync(m => m.Id == request.MessageId, cancellationToken);

        if (message == null)
            throw new InvalidOperationException("Message not found");

        if (message.IsClosed)
            throw new InvalidOperationException("Message is closed and no longer accepting responses");

        // Check if user has already responded
        var existingResponse = await context.MessageResponses
            .FirstOrDefaultAsync(r => r.MessageId == request.MessageId && r.SessionId == request.SessionId, cancellationToken);

        if (existingResponse != null)
        {
            // For reactions, allow toggling (update existing response)
            if (message.ResponseType == MessageResponseType.Reactions)
            {
                existingResponse.Reaction = request.Reaction;
                await context.SaveChangesAsync(cancellationToken);
                return existingResponse.Id;
            }
            else
            {
                // For other types, don't allow changing responses
                throw new InvalidOperationException("You have already responded to this message");
            }
        }

        // Validate response based on type
        if (message.ResponseType == MessageResponseType.Reactions)
        {
            var validReactions = new[] { "👍", "👎", "❤️", "😂", "😮" };
            if (string.IsNullOrEmpty(request.Reaction) || !validReactions.Contains(request.Reaction))
                throw new ArgumentException("Invalid reaction");
        }
        else if (message.ResponseType == MessageResponseType.PredefinedAnswers)
        {
            if (!request.SelectedOptionId.HasValue)
                throw new ArgumentException("Must select an option");
        }
        else if (message.ResponseType == MessageResponseType.FreeText)
        {
            if (string.IsNullOrWhiteSpace(request.FreeText) || request.FreeText.Length > 500)
                throw new ArgumentException("Free text response must be between 1 and 500 characters");
        }

        var response = new MessageResponse
        {
            Id = Guid.NewGuid(),
            MessageId = request.MessageId,
            SessionId = request.SessionId,
            Reaction = request.Reaction,
            SelectedOptionId = request.SelectedOptionId,
            FreeText = string.IsNullOrWhiteSpace(request.FreeText) ? null : HtmlEncoder.Default.Encode(request.FreeText),
            CreatedAt = DateTime.UtcNow
        };

        context.MessageResponses.Add(response);
        await context.SaveChangesAsync(cancellationToken);

        // Notify all clients so the facilitator sees live response counts
        await _hubContext.Clients.Group(message.MeetingId.ToString())
            .SendAsync("MeetingUpdated", "message_response", cancellationToken);

        return response.Id;
    }
}
