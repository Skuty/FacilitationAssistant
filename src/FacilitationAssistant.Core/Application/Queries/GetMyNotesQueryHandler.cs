using FacilitationAssistant.Core.Application.DTOs;
using FacilitationAssistant.Core.Interfaces;
using MediatR;

namespace FacilitationAssistant.Core.Application.Queries;

public class GetMyNotesQueryHandler : IRequestHandler<GetMyNotesQuery, List<NoteDto>>
{
    private readonly IMeetingRepository _repository;

    public GetMyNotesQueryHandler(IMeetingRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<NoteDto>> Handle(GetMyNotesQuery request, CancellationToken ct)
    {
        var meeting = await _repository.GetByIdAsync(request.MeetingId, ct);
        if (meeting == null)
            return new List<NoteDto>();

        return meeting.Notes
            .Where(n => n.OwnerId == request.OwnerId)
            .Select(n => new NoteDto
            {
                Id = n.Id,
                Content = n.Content,
                OwnerName = n.OwnerName,
                IsPrivate = n.IsPrivate,
                LinkedStageId = n.LinkedStageId,
                CreatedAt = n.CreatedAt,
                LastEditedAt = n.LastEditedAt
            }).ToList();
    }
}
