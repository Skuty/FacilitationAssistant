using FacilitationAssistant.Core.Application.Notifications;
using FacilitationAssistant.Core.Domain.Common;
using FacilitationAssistant.Core.Interfaces;
using MediatR;

namespace FacilitationAssistant.Core.Application.Commands;

public class StartStageCommandHandler : IRequestHandler<StartStageCommand, Result>
{
    private readonly IMeetingRepository _repository;
    private readonly IDateTimeProvider _clock;
    private readonly IPublisher _publisher;

    public StartStageCommandHandler(
        IMeetingRepository repository,
        IDateTimeProvider clock,
        IPublisher publisher)
    {
        _repository = repository;
        _clock = clock;
        _publisher = publisher;
    }

    public async Task<Result> Handle(StartStageCommand request, CancellationToken ct)
    {
        var meeting = await _repository.GetByIdAsync(request.MeetingId, ct);
        if (meeting == null)
            return Result.Failure("Meeting not found");

        try
        {
            meeting.StartStage(request.StageId, _clock);
            await _repository.UpdateAsync(meeting, ct);
            await _publisher.Publish(new MeetingUpdatedNotification(meeting.Id), ct);
            return Result.Success();
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
        {
            return Result.Failure(ex.Message);
        }
    }
}
