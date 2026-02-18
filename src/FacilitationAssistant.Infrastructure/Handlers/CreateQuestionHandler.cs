using FacilitationAssistant.Core.Commands;
using FacilitationAssistant.Core.Entities;
using FacilitationAssistant.Infrastructure.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;
using System.Text.Encodings.Web;

namespace FacilitationAssistant.Infrastructure.Handlers;

/// <summary>
/// Handles creating a new question for a meeting.
/// </summary>
public class CreateQuestionHandler : IRequestHandler<CreateQuestionCommand, Guid>
{
    private readonly IDbContextFactory<FacilitationDbContext> _contextFactory;

    public CreateQuestionHandler(IDbContextFactory<FacilitationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async ValueTask<Guid> Handle(CreateQuestionCommand request, CancellationToken cancellationToken)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        
        var meeting = await context.Meetings
            .FirstOrDefaultAsync(m => m.Id == request.MeetingId, cancellationToken);

        if (meeting == null)
            throw new InvalidOperationException("Meeting not found");

        // Validate question text length
        if (string.IsNullOrWhiteSpace(request.Text) || request.Text.Length < 10 || request.Text.Length > 300)
            throw new ArgumentException("Question text must be between 10 and 300 characters");

        // Validate options for choice-based questions
        if (request.AnswerType == QuestionAnswerType.SingleChoice || 
            request.AnswerType == QuestionAnswerType.MultipleChoice)
        {
            if (request.Options == null || request.Options.Count < 2 || request.Options.Count > 10)
                throw new ArgumentException("Choice questions must have between 2 and 10 options");

            if (request.Options.Any(o => string.IsNullOrWhiteSpace(o) || o.Length > 100))
                throw new ArgumentException("Option text must be between 1 and 100 characters");
        }

        // Validate scale questions
        if (request.AnswerType == QuestionAnswerType.Scale)
        {
            if (!request.ScaleMin.HasValue || !request.ScaleMax.HasValue)
                throw new ArgumentException("Scale questions must have min and max values");

            if (request.ScaleMax.Value - request.ScaleMin.Value < 1)
                throw new ArgumentException("Scale range must have at least 2 points");

            if (request.ScaleMax.Value - request.ScaleMin.Value > 9)
                throw new ArgumentException("Scale range cannot exceed 10 points");
        }

        var question = new Question
        {
            Id = Guid.NewGuid(),
            MeetingId = request.MeetingId,
            Text = HtmlEncoder.Default.Encode(request.Text),
            AnswerType = request.AnswerType,
            TriggerType = request.TriggerType,
            AssociatedStageId = request.AssociatedStageId,
            ResultVisibility = request.ResultVisibility,
            ScaleMin = request.ScaleMin,
            ScaleMax = request.ScaleMax,
            ScaleMinLabel = string.IsNullOrWhiteSpace(request.ScaleMinLabel) ? null : HtmlEncoder.Default.Encode(request.ScaleMinLabel),
            ScaleMaxLabel = string.IsNullOrWhiteSpace(request.ScaleMaxLabel) ? null : HtmlEncoder.Default.Encode(request.ScaleMaxLabel),
            MaxSelectableOptions = request.MaxSelectableOptions,
            Status = request.TriggerImmediately ? QuestionStatus.Active : QuestionStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            TriggeredAt = request.TriggerImmediately ? DateTime.UtcNow : null
        };

        context.Questions.Add(question);
        
        // Add options for choice-based questions
        if (request.Options != null && request.Options.Any())
        {
            for (int i = 0; i < request.Options.Count; i++)
            {
                var option = new QuestionOption
                {
                    Id = Guid.NewGuid(),
                    QuestionId = question.Id,
                    OptionText = HtmlEncoder.Default.Encode(request.Options[i]),
                    OrderIndex = i
                };
                context.QuestionOptions.Add(option);
            }
        }

        await context.SaveChangesAsync(cancellationToken);

        return question.Id;
    }
}
