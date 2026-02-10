using FacilitationAssistant.Core.Commands;
using FacilitationAssistant.Core.Entities;
using FacilitationAssistant.Infrastructure.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;
using System.Text.Encodings.Web;

namespace FacilitationAssistant.Infrastructure.Handlers;

public class RespondToMessageHandler : IRequestHandler<RespondToMessageCommand, Guid>
{
    private readonly FacilitationDbContext _context;

    public RespondToMessageHandler(FacilitationDbContext context)
    {
        _context = context;
    }

    public async ValueTask<Guid> Handle(RespondToMessageCommand request, CancellationToken cancellationToken)
    {
        var message = await _context.Messages
            .Include(m => m.Responses)
            .FirstOrDefaultAsync(m => m.Id == request.MessageId, cancellationToken);

        if (message == null)
            throw new InvalidOperationException("Message not found");

        if (message.IsClosed)
            throw new InvalidOperationException("Message is closed and no longer accepting responses");

        // Check if user has already responded
        var existingResponse = message.Responses
            .FirstOrDefault(r => r.SessionId == request.SessionId);

        if (existingResponse != null)
        {
            // For reactions, allow toggling (update existing response)
            if (message.ResponseType == MessageResponseType.Reactions)
            {
                existingResponse.Reaction = request.Reaction;
                await _context.SaveChangesAsync(cancellationToken);
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

        _context.MessageResponses.Add(response);
        await _context.SaveChangesAsync(cancellationToken);
        return response.Id;
    }
}
