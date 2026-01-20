using FacilitationAssistant.Core.Application.Notifications;
using FacilitationAssistant.Core.Domain.Common;
using FacilitationAssistant.Core.Interfaces;
using MediatR;

namespace FacilitationAssistant.Core.Application.Commands;

public class StartMeetingCommandHandler : IRequestHandler<StartMeetingCommand, Result>
{
    private readonly IMeetingRepository _repository;
    private readonly IDateTimeProvider _clock;
    private readonly IPublisher _publisher;

    public StartMeetingCommandHandler(
        IMeetingRepository repository,
        IDateTimeProvider clock,
        IPublisher publisher)
    {
        _repository = repository;
        _clock = clock;
        _publisher = publisher;
    }

    public async Task<Result> Handle(StartMeetingCommand request, CancellationToken ct)
    {
        var meeting = await _repository.GetByIdAsync(request.MeetingId, ct);
        if (meeting == null)
            return Result.Failure("Meeting not found");

        try
        {
            meeting.Start(_clock);
            await _repository.UpdateAsync(meeting, ct);
            await _publisher.Publish(new MeetingUpdatedNotification(meeting.Id), ct);
            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}
