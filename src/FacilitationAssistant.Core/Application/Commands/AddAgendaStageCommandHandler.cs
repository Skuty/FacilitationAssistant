using FacilitationAssistant.Core.Application.Notifications;
using FacilitationAssistant.Core.Domain.Common;
using FacilitationAssistant.Core.Interfaces;
using MediatR;

namespace FacilitationAssistant.Core.Application.Commands;

public class AddAgendaStageCommandHandler : IRequestHandler<AddAgendaStageCommand, Result>
{
    private readonly IMeetingRepository _repository;
    private readonly IPublisher _publisher;

    public AddAgendaStageCommandHandler(IMeetingRepository repository, IPublisher publisher)
    {
        _repository = repository;
        _publisher = publisher;
    }

    public async Task<Result> Handle(AddAgendaStageCommand request, CancellationToken ct)
    {
        var meeting = await _repository.GetByIdAsync(request.MeetingId, ct);
        if (meeting == null)
            return Result.Failure("Meeting not found");

        meeting.AddStage(request.Title, request.Description, request.DurationMinutes, request.SortOrder);
        await _repository.UpdateAsync(meeting, ct);
        await _publisher.Publish(new MeetingUpdatedNotification(meeting.Id), ct);

        return Result.Success();
    }
}
