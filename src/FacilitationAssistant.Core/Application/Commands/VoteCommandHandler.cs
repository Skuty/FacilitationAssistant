using FacilitationAssistant.Core.Application.Notifications;
using FacilitationAssistant.Core.Domain.Common;
using FacilitationAssistant.Core.Interfaces;
using MediatR;

namespace FacilitationAssistant.Core.Application.Commands;

public class VoteCommandHandler : IRequestHandler<VoteCommand, Result>
{
    private readonly IMeetingRepository _repository;
    private readonly IPublisher _publisher;

    public VoteCommandHandler(IMeetingRepository repository, IPublisher publisher)
    {
        _repository = repository;
        _publisher = publisher;
    }

    public async Task<Result> Handle(VoteCommand request, CancellationToken ct)
    {
        var meeting = await _repository.GetByIdAsync(request.MeetingId, ct);
        if (meeting == null)
            return Result.Failure("Meeting not found");

        var poll = meeting.Polls.FirstOrDefault(p => p.Id == request.PollId);
        if (poll == null)
            return Result.Failure("Poll not found");

        try
        {
            poll.AddVote(request.VoterSessionId, request.OptionId);
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
