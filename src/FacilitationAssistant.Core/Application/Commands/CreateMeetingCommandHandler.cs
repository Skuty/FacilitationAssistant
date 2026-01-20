using FacilitationAssistant.Core.Application.Notifications;
using FacilitationAssistant.Core.Domain.Aggregates;
using FacilitationAssistant.Core.Domain.Common;
using FacilitationAssistant.Core.Interfaces;
using MediatR;

namespace FacilitationAssistant.Core.Application.Commands;

public class CreateMeetingCommandHandler : IRequestHandler<CreateMeetingCommand, Result<Guid>>
{
    private readonly IMeetingRepository _repository;
    private readonly IPublisher _publisher;

    public CreateMeetingCommandHandler(IMeetingRepository repository, IPublisher publisher)
    {
        _repository = repository;
        _publisher = publisher;
    }

    public async Task<Result<Guid>> Handle(CreateMeetingCommand request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return Result<Guid>.Failure("Meeting title is required");

        var meeting = new Meeting
        {
            Title = request.Title
        };

        await _repository.AddAsync(meeting, ct);
        await _publisher.Publish(new MeetingUpdatedNotification(meeting.Id), ct);

        return Result<Guid>.Success(meeting.Id);
    }
}
