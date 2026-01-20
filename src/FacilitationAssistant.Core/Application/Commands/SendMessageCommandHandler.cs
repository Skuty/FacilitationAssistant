using FacilitationAssistant.Core.Application.Notifications;
using FacilitationAssistant.Core.Domain.Common;
using FacilitationAssistant.Core.Interfaces;
using MediatR;

namespace FacilitationAssistant.Core.Application.Commands;

public class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, Result<Guid>>
{
    private readonly IMeetingRepository _repository;
    private readonly IPublisher _publisher;

    public SendMessageCommandHandler(IMeetingRepository repository, IPublisher publisher)
    {
        _repository = repository;
        _publisher = publisher;
    }

    public async Task<Result<Guid>> Handle(SendMessageCommand request, CancellationToken ct)
    {
        var meeting = await _repository.GetByIdAsync(request.MeetingId, ct);
        if (meeting == null)
            return Result<Guid>.Failure("Meeting not found");

        var messageId = Guid.NewGuid();
        meeting.AddMessage(request.Content, request.SenderSessionId, request.SenderName, request.Type);
        await _repository.UpdateAsync(meeting, ct);
        await _publisher.Publish(new MeetingUpdatedNotification(meeting.Id), ct);

        return Result<Guid>.Success(messageId);
    }
}
