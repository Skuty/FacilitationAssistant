using FacilitationAssistant.Core.Application.DTOs;
using FacilitationAssistant.Core.Interfaces;
using MediatR;

namespace FacilitationAssistant.Core.Application.Queries;

public class GetMeetingStateQueryHandler : IRequestHandler<GetMeetingStateQuery, MeetingStateDto?>
{
    private readonly IMeetingRepository _repository;

    public GetMeetingStateQueryHandler(IMeetingRepository repository)
    {
        _repository = repository;
    }

    public async Task<MeetingStateDto?> Handle(GetMeetingStateQuery request, CancellationToken ct)
    {
        var meeting = await _repository.GetByIdAsync(request.MeetingId, ct);
        if (meeting == null)
            return null;

        return new MeetingStateDto
        {
            Id = meeting.Id,
            Title = meeting.Title,
            State = meeting.State,
            StartedAt = meeting.StartedAt,
            CurrentStageId = meeting.CurrentStageId,
            Stages = meeting.Stages.Select(s => new AgendaStageDto
            {
                Id = s.Id,
                Title = s.Title,
                Description = s.Description,
                DurationMinutes = s.DurationMinutes,
                Status = s.Status,
                StartedAt = s.StartedAt,
                CompletedAt = s.CompletedAt
            }).ToList(),
            Attendees = meeting.Attendees.Select(a => new AttendeeDto
            {
                Id = a.Id,
                DisplayName = a.DisplayName,
                JoinTime = a.JoinTime
            }).ToList(),
            Polls = meeting.Polls.Select(p => new PollDto
            {
                Id = p.Id,
                Question = p.Question,
                Status = p.Status,
                AllowMultipleVotes = p.AllowMultipleVotes,
                ShowResultsBeforeClose = p.ShowResultsBeforeClose,
                Options = p.Options.Select(o => new PollOptionDto
                {
                    Id = o.Id,
                    Text = o.Text,
                    VoteCount = p.Votes.Count(v => v.OptionId == o.Id)
                }).ToList(),
                Votes = p.Votes.Select(v => new VoteDto
                {
                    Id = v.Id,
                    OptionId = v.OptionId
                }).ToList()
            }).ToList(),
            Messages = meeting.Messages.Select(m => new MessageDto
            {
                Id = m.Id,
                Content = m.Content,
                SenderName = m.SenderName,
                Type = m.Type,
                IsClosed = m.IsClosed,
                Timestamp = m.Timestamp
            }).ToList(),
            Concerns = meeting.Concerns.Select(c => new ConcernDto
            {
                Id = c.Id,
                Content = c.Content,
                RaisedByName = c.RaisedByName,
                Severity = c.Severity,
                Status = c.Status,
                FacilitatorResponse = c.FacilitatorResponse
            }).ToList(),
            PublicNotes = meeting.Notes
                .Where(n => !n.IsPrivate)
                .Select(n => new NoteDto
                {
                    Id = n.Id,
                    Content = n.Content,
                    OwnerName = n.OwnerName,
                    IsPrivate = n.IsPrivate,
                    LinkedStageId = n.LinkedStageId,
                    CreatedAt = n.CreatedAt,
                    LastEditedAt = n.LastEditedAt
                }).ToList()
        };
    }
}
