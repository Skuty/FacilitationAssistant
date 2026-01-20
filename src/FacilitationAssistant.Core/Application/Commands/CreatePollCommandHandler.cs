using FacilitationAssistant.Core.Application.Notifications;
using FacilitationAssistant.Core.Domain.Common;
using FacilitationAssistant.Core.Domain.Entities;
using FacilitationAssistant.Core.Interfaces;
using MediatR;

namespace FacilitationAssistant.Core.Application.Commands;

public class CreatePollCommandHandler : IRequestHandler<CreatePollCommand, Result<Guid>>
{
    private readonly IMeetingRepository _repository;
    private readonly IPublisher _publisher;

    public CreatePollCommandHandler(IMeetingRepository repository, IPublisher publisher)
    {
        _repository = repository;
        _publisher = publisher;
    }

    public async Task<Result<Guid>> Handle(CreatePollCommand request, CancellationToken ct)
    {
        var meeting = await _repository.GetByIdAsync(request.MeetingId, ct);
        if (meeting == null)
            return Result<Guid>.Failure("Meeting not found");

        var poll = new Poll
        {
            Question = request.Question,
            StageId = request.StageId,
            AllowMultipleVotes = request.AllowMultipleVotes,
            ShowResultsBeforeClose = request.ShowResultsBeforeClose
        };

        for (int i = 0; i < request.Options.Count; i++)
        {
            poll.AddOption(request.Options[i], i);
        }

        meeting.AddPoll(poll);
        await _repository.UpdateAsync(meeting, ct);
        await _publisher.Publish(new MeetingUpdatedNotification(meeting.Id), ct);

        return Result<Guid>.Success(poll.Id);
    }
}
