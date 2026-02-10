using FacilitationAssistant.Core.Entities;
using FacilitationAssistant.Core.Queries;
using FacilitationAssistant.Infrastructure.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FacilitationAssistant.Infrastructure.Handlers;

public class GetMessagesByMeetingHandler : IRequestHandler<GetMessagesByMeetingQuery, List<MessageDto>>
{
    private readonly FacilitationDbContext _context;

    public GetMessagesByMeetingHandler(FacilitationDbContext context)
    {
        _context = context;
    }

    public async ValueTask<List<MessageDto>> Handle(GetMessagesByMeetingQuery request, CancellationToken cancellationToken)
    {
        var messages = await _context.Messages
            .Where(m => m.MeetingId == request.MeetingId)
            .Include(m => m.Responses)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(cancellationToken);

        return messages.Select(m => new MessageDto(
            m.Id,
            m.Text,
            m.Type,
            m.ResponseType,
            m.CreatedAt,
            m.IsClosed,
            m.Responses.Count,
            null // UserHasResponded is null for this query (no session context)
        )).ToList();
    }
}

public class GetMessageByIdHandler : IRequestHandler<GetMessageByIdQuery, MessageDetailDto?>
{
    private readonly FacilitationDbContext _context;

    public GetMessageByIdHandler(FacilitationDbContext context)
    {
        _context = context;
    }

    public async ValueTask<MessageDetailDto?> Handle(GetMessageByIdQuery request, CancellationToken cancellationToken)
    {
        var message = await _context.Messages
            .Include(m => m.Options)
            .Include(m => m.Responses)
            .FirstOrDefaultAsync(m => m.Id == request.MessageId, cancellationToken);

        if (message == null)
            return null;

        var optionDtos = message.Options
            .OrderBy(o => o.OrderIndex)
            .Select(o => new MessageOptionDto(o.Id, o.Text, o.OrderIndex))
            .ToList();

        var responseDtos = message.Responses
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
            summary
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

public class GetPendingMessagesHandler : IRequestHandler<GetPendingMessagesQuery, List<MessageDto>>
{
    private readonly FacilitationDbContext _context;

    public GetPendingMessagesHandler(FacilitationDbContext context)
    {
        _context = context;
    }

    public async ValueTask<List<MessageDto>> Handle(GetPendingMessagesQuery request, CancellationToken cancellationToken)
    {
        var messages = await _context.Messages
            .Where(m => m.MeetingId == request.MeetingId && !m.IsClosed)
            .Include(m => m.Responses)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(cancellationToken);

        return messages.Select(m => new MessageDto(
            m.Id,
            m.Text,
            m.Type,
            m.ResponseType,
            m.CreatedAt,
            m.IsClosed,
            m.Responses.Count,
            m.Responses.Any(r => r.SessionId == request.SessionId)
        )).ToList();
    }
}
