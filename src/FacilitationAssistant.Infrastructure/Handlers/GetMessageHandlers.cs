using FacilitationAssistant.Core.Entities;
using FacilitationAssistant.Core.Queries;
using FacilitationAssistant.Infrastructure.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FacilitationAssistant.Infrastructure.Handlers;

/// <summary>
/// Handles retrieving all messages for a meeting.
/// </summary>
public class GetMessagesByMeetingHandler : IRequestHandler<GetMessagesByMeetingQuery, List<MessageDto>>
{
    private readonly IDbContextFactory<FacilitationDbContext> _contextFactory;

    public GetMessagesByMeetingHandler(IDbContextFactory<FacilitationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async ValueTask<List<MessageDto>> Handle(GetMessagesByMeetingQuery request, CancellationToken cancellationToken)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        
        var messages = await context.Messages
            .Where(m => m.MeetingId == request.MeetingId)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(cancellationToken);

        var messageIds = messages.Select(m => m.Id).ToList();
        
        // Get response counts for all messages
        var responseCounts = await context.MessageResponses
            .Where(r => messageIds.Contains(r.MessageId))
            .GroupBy(r => r.MessageId)
            .Select(g => new { MessageId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.MessageId, x => x.Count, cancellationToken);

        return messages.Select(m => new MessageDto(
            m.Id,
            m.Text,
            m.Type,
            m.ResponseType,
            m.CreatedAt,
            m.IsClosed,
            responseCounts.ContainsKey(m.Id) ? responseCounts[m.Id] : 0,
            null, // UserHasResponded is null for this query (no session context)
            m.AuthorName
        )).ToList();
    }
}

/// <summary>
/// Handles retrieving detailed information about a specific message.
/// </summary>
public class GetMessageByIdHandler : IRequestHandler<GetMessageByIdQuery, MessageDetailDto?>
{
    private readonly IDbContextFactory<FacilitationDbContext> _contextFactory;

    public GetMessageByIdHandler(IDbContextFactory<FacilitationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async ValueTask<MessageDetailDto?> Handle(GetMessageByIdQuery request, CancellationToken cancellationToken)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        
        var message = await context.Messages
            .FirstOrDefaultAsync(m => m.Id == request.MessageId, cancellationToken);

        if (message == null)
            return null;

        var options = await context.MessageOptions
            .Where(o => o.MessageId == request.MessageId)
            .OrderBy(o => o.OrderIndex)
            .ToListAsync(cancellationToken);

        var responses = await context.MessageResponses
            .Where(r => r.MessageId == request.MessageId)
            .ToListAsync(cancellationToken);

        var optionDtos = options
            .Select(o => new MessageOptionDto(o.Id, o.Text, o.OrderIndex))
            .ToList();

        var responseDtos = responses
            .Select(r => new MessageResponseDto(
                r.Id,
                r.SessionId,
                r.CreatedAt,
                r.Reaction,
                r.SelectedOptionId,
                r.FreeText
            ))
            .ToList();

        var summary = BuildResponseSummary(message, responseDtos);

        return new MessageDetailDto(
            message.Id,
            message.Text,
            message.Type,
            message.ResponseType,
            message.CreatedAt,
            message.IsClosed,
            optionDtos,
            responseDtos,
            summary,
            message.AuthorName
        );
    }

    private ResponseSummaryDto? BuildResponseSummary(Message message, List<MessageResponseDto> responses)
    {
        if (!responses.Any())
            return null;

        Dictionary<string, int>? reactionCounts = null;
        Dictionary<Guid, int>? optionCounts = null;
        List<string>? freeTextResponses = null;

        if (message.ResponseType == MessageResponseType.Reactions)
        {
            reactionCounts = responses
                .Where(r => !string.IsNullOrEmpty(r.Reaction))
                .GroupBy(r => r.Reaction!)
                .ToDictionary(g => g.Key, g => g.Count());
        }
        else if (message.ResponseType == MessageResponseType.PredefinedAnswers)
        {
            optionCounts = responses
                .Where(r => r.SelectedOptionId.HasValue)
                .GroupBy(r => r.SelectedOptionId!.Value)
                .ToDictionary(g => g.Key, g => g.Count());
        }
        else if (message.ResponseType == MessageResponseType.FreeText)
        {
            freeTextResponses = responses
                .Where(r => !string.IsNullOrEmpty(r.FreeText))
                .Select(r => r.FreeText!)
                .ToList();
        }

        return new ResponseSummaryDto(
            responses.Count,
            reactionCounts,
            optionCounts,
            freeTextResponses
        );
    }
}

/// <summary>
/// Handles retrieving pending (not closed) messages for a meeting and session.
/// </summary>
public class GetPendingMessagesHandler : IRequestHandler<GetPendingMessagesQuery, List<MessageDto>>
{
    private readonly IDbContextFactory<FacilitationDbContext> _contextFactory;

    public GetPendingMessagesHandler(IDbContextFactory<FacilitationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async ValueTask<List<MessageDto>> Handle(GetPendingMessagesQuery request, CancellationToken cancellationToken)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        
        var messages = await context.Messages
            .Where(m => m.MeetingId == request.MeetingId && !m.IsClosed)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(cancellationToken);

        var messageIds = messages.Select(m => m.Id).ToList();
        
        // Get response counts and user responses for all messages
        var allResponses = await context.MessageResponses
            .Where(r => messageIds.Contains(r.MessageId))
            .ToListAsync(cancellationToken);

        var responseCounts = allResponses
            .GroupBy(r => r.MessageId)
            .ToDictionary(g => g.Key, g => g.Count());

        var userResponses = allResponses
            .Where(r => r.SessionId == request.SessionId)
            .Select(r => r.MessageId)
            .ToHashSet();

        return messages.Select(m => new MessageDto(
            m.Id,
            m.Text,
            m.Type,
            m.ResponseType,
            m.CreatedAt,
            m.IsClosed,
            responseCounts.ContainsKey(m.Id) ? responseCounts[m.Id] : 0,
            userResponses.Contains(m.Id),
            m.AuthorName
        )).ToList();
    }
}
