using FacilitationAssistant.Core.Application.Notifications;
using FacilitationAssistant.Core.Domain.Common;
using FacilitationAssistant.Core.Interfaces;
using MediatR;

namespace FacilitationAssistant.Core.Application.Commands;

public class OpenPollCommandHandler : IRequestHandler<OpenPollCommand, Result>
{
    private readonly IMeetingRepository _repository;
    private readonly IDateTimeProvider _clock;
    private readonly IPublisher _publisher;

    public OpenPollCommandHandler(
        IMeetingRepository repository,
        IDateTimeProvider clock,
        IPublisher publisher)
    {
        _repository = repository;
        _clock = clock;
        _publisher = publisher;
    }

    public async Task<Result> Handle(OpenPollCommand request, CancellationToken ct)
    {
        var meeting = await _repository.GetByIdAsync(request.MeetingId, ct);
        if (meeting == null)
            return Result.Failure("Meeting not found");

        var poll = meeting.Polls.FirstOrDefault(p => p.Id == request.PollId);
        if (poll == null)
            return Result.Failure("Poll not found");

        try
        {
            poll.Open(_clock.UtcNow);
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
