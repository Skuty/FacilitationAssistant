using FacilitationAssistant.Core.Entities;
using Mediator;

namespace FacilitationAssistant.Core.Queries;

public record GetMessagesByMeetingQuery(Guid MeetingId) : IRequest<List<MessageDto>>;

public record GetMessageByIdQuery(Guid MessageId, string? SessionId = null) : IRequest<MessageDetailDto?>;

public record GetPendingMessagesQuery(Guid MeetingId, string SessionId) : IRequest<List<MessageDto>>;

// DTOs
public record MessageDto(
    Guid Id,
    string Text,
    MessageType Type,
    MessageResponseType? ResponseType,
    DateTime CreatedAt,
    bool IsClosed,
    int ResponseCount,
    bool? UserHasResponded
);

public record MessageDetailDto(
    Guid Id,
    string Text,
    MessageType Type,
    MessageResponseType? ResponseType,
    DateTime CreatedAt,
    bool IsClosed,
    List<MessageOptionDto> Options,
    List<MessageResponseDto> Responses,
    ResponseSummaryDto? Summary
);

public record MessageOptionDto(
    Guid Id,
    string Text,
    int OrderIndex
);

public record MessageResponseDto(
    Guid Id,
    string SessionId,
    DateTime CreatedAt,
    string? Reaction,
    Guid? SelectedOptionId,
    string? FreeText
);

public record ResponseSummaryDto(
    int TotalResponses,
    Dictionary<string, int>? ReactionCounts,
    Dictionary<Guid, int>? OptionCounts,
    List<string>? FreeTextResponses
);
