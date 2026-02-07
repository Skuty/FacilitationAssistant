using FacilitationAssistant.Core.Commands;
using FacilitationAssistant.Core.Entities;
using FacilitationAssistant.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FacilitationAssistant.Infrastructure.Handlers;

public class TriggerQuestionHandler : IRequestHandler<TriggerQuestionCommand, bool>
{
    private readonly FacilitationDbContext _context;

    public TriggerQuestionHandler(FacilitationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(TriggerQuestionCommand request, CancellationToken cancellationToken)
    {
        var question = await _context.Questions
            .FirstOrDefaultAsync(q => q.Id == request.QuestionId, cancellationToken);

        if (question == null)
            return false;

        if (question.Status == QuestionStatus.Active)
            return true; // Already active

        question.Status = QuestionStatus.Active;
        question.TriggeredAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
