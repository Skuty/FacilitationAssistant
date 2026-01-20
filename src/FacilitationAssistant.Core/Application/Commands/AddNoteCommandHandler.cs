using FacilitationAssistant.Core.Application.Notifications;
using FacilitationAssistant.Core.Domain.Common;
using FacilitationAssistant.Core.Interfaces;
using MediatR;

namespace FacilitationAssistant.Core.Application.Commands;

public class AddNoteCommandHandler : IRequestHandler<AddNoteCommand, Result<Guid>>
{
    private readonly IMeetingRepository _repository;
    private readonly IPublisher _publisher;

    public AddNoteCommandHandler(IMeetingRepository repository, IPublisher publisher)
    {
        _repository = repository;
        _publisher = publisher;
    }

    public async Task<Result<Guid>> Handle(AddNoteCommand request, CancellationToken ct)
    {
        var meeting = await _repository.GetByIdAsync(request.MeetingId, ct);
        if (meeting == null)
            return Result<Guid>.Failure("Meeting not found");

        var noteId = Guid.NewGuid();
        meeting.AddNote(
            request.Content,
            request.OwnerId,
            request.OwnerName,
            request.IsPrivate,
            request.LinkedStageId);

        await _repository.UpdateAsync(meeting, ct);
        await _publisher.Publish(new MeetingUpdatedNotification(meeting.Id), ct);

        return Result<Guid>.Success(noteId);
    }
}
