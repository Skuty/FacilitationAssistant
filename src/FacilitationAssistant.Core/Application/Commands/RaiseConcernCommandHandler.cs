using FacilitationAssistant.Core.Application.Notifications;
using FacilitationAssistant.Core.Domain.Common;
using FacilitationAssistant.Core.Interfaces;
using MediatR;

namespace FacilitationAssistant.Core.Application.Commands;

public class RaiseConcernCommandHandler : IRequestHandler<RaiseConcernCommand, Result<Guid>>
{
    private readonly IMeetingRepository _repository;
    private readonly IPublisher _publisher;

    public RaiseConcernCommandHandler(IMeetingRepository repository, IPublisher publisher)
    {
        _repository = repository;
        _publisher = publisher;
    }

    public async Task<Result<Guid>> Handle(RaiseConcernCommand request, CancellationToken ct)
    {
        var meeting = await _repository.GetByIdAsync(request.MeetingId, ct);
        if (meeting == null)
            return Result<Guid>.Failure("Meeting not found");

        var concernId = Guid.NewGuid();
        meeting.AddConcern(
            request.Content,
            request.RaisedBySessionId,
            request.RaisedByName,
            request.IsAnonymous,
            request.Severity);

        await _repository.UpdateAsync(meeting, ct);
        await _publisher.Publish(new MeetingUpdatedNotification(meeting.Id), ct);

        return Result<Guid>.Success(concernId);
    }
}
