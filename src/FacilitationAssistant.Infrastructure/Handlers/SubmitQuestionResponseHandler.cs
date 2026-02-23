using FacilitationAssistant.Core.Commands;
using FacilitationAssistant.Core.Entities;
using FacilitationAssistant.Infrastructure.Data;
using FacilitationAssistant.Infrastructure.Hubs;
using Mediator;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Text.Encodings.Web;

namespace FacilitationAssistant.Infrastructure.Handlers;

/// <summary>
/// Handles submitting an attendee's response to a question.
/// </summary>
public class SubmitQuestionResponseHandler : IRequestHandler<SubmitQuestionResponseCommand, Guid>
{
    private readonly IDbContextFactory<FacilitationDbContext> _contextFactory;
    private readonly IHubContext<MeetingHub> _hubContext;

    public SubmitQuestionResponseHandler(IDbContextFactory<FacilitationDbContext> contextFactory, IHubContext<MeetingHub> hubContext)
    {
        _contextFactory = contextFactory;
        _hubContext = hubContext;
    }

    public async ValueTask<Guid> Handle(SubmitQuestionResponseCommand request, CancellationToken cancellationToken)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        
        var question = await context.Questions
            .FirstOrDefaultAsync(q => q.Id == request.QuestionId, cancellationToken);

        if (question == null)
            throw new InvalidOperationException("Question not found");

        if (question.Status != QuestionStatus.Active)
            throw new InvalidOperationException("Question is not active");

        // Check if attendee already answered this question
        var existingResponse = await context.QuestionResponses
            .FirstOrDefaultAsync(r => r.QuestionId == request.QuestionId && r.AttendeeSessionId == request.AttendeeSessionId, cancellationToken);

        if (existingResponse != null)
            throw new InvalidOperationException("You have already answered this question");

        if (request.AuthorName != null && request.AuthorName.Length > 50)
            throw new ArgumentException("Author name must not exceed 50 characters.");

        var response = new QuestionResponse
        {
            Id = Guid.NewGuid(),
            QuestionId = request.QuestionId,
            AttendeeSessionId = request.AttendeeSessionId,
            AuthorName = string.IsNullOrWhiteSpace(request.AuthorName) ? null : HtmlEncoder.Default.Encode(request.AuthorName),
            AnswerChoiceIds = request.AnswerChoiceIds ?? new List<string>(),
            AnswerText = string.IsNullOrWhiteSpace(request.AnswerText) ? null : HtmlEncoder.Default.Encode(request.AnswerText),
            AnswerScaleValue = request.AnswerScaleValue,
            Status = request.Status,
            SubmittedAt = DateTime.UtcNow
        };

        context.QuestionResponses.Add(response);
        await context.SaveChangesAsync(cancellationToken);
        
        // Notify all clients
        await _hubContext.Clients.Group(question.MeetingId.ToString())
            .SendAsync("MeetingUpdated", "question_response", cancellationToken);

        return response.Id;
    }
}
