using FacilitationAssistant.Core.Commands;
using FacilitationAssistant.Core.Entities;
using FacilitationAssistant.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FacilitationAssistant.Infrastructure.Handlers;

public class SubmitQuestionResponseHandler : IRequestHandler<SubmitQuestionResponseCommand, Guid>
{
    private readonly FacilitationDbContext _context;

    public SubmitQuestionResponseHandler(FacilitationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(SubmitQuestionResponseCommand request, CancellationToken cancellationToken)
    {
        var question = await _context.Questions
            .FirstOrDefaultAsync(q => q.Id == request.QuestionId, cancellationToken);

        if (question == null)
            throw new InvalidOperationException("Question not found");

        if (question.Status != QuestionStatus.Active)
            throw new InvalidOperationException("Question is not active");

        // Check if attendee already answered this question
        var existingResponse = await _context.QuestionResponses
            .FirstOrDefaultAsync(r => r.QuestionId == request.QuestionId && 
                                     r.AttendeeSessionId == request.AttendeeSessionId, 
                                cancellationToken);

        if (existingResponse != null)
            throw new InvalidOperationException("You have already answered this question");

        var response = new QuestionResponse
        {
            Id = Guid.NewGuid(),
            QuestionId = request.QuestionId,
            AttendeeSessionId = request.AttendeeSessionId,
            AnswerChoiceIds = request.AnswerChoiceIds ?? new List<Guid>(),
            AnswerText = request.AnswerText,
            AnswerScaleValue = request.AnswerScaleValue,
            Status = request.Status,
            SubmittedAt = DateTime.UtcNow
        };

        _context.QuestionResponses.Add(response);
        await _context.SaveChangesAsync(cancellationToken);

        return response.Id;
    }
}
