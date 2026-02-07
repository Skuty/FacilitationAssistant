using FacilitationAssistant.Core.Commands;
using FacilitationAssistant.Core.Entities;
using FacilitationAssistant.Infrastructure.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FacilitationAssistant.Infrastructure.Handlers;

public class CloseQuestionHandler : IRequestHandler<CloseQuestionCommand, bool>
{
    private readonly FacilitationDbContext _context;

    public CloseQuestionHandler(FacilitationDbContext context)
    {
        _context = context;
    }

    public async ValueTask<bool> Handle(CloseQuestionCommand request, CancellationToken cancellationToken)
    {
        var question = await _context.Questions
            .FirstOrDefaultAsync(q => q.Id == request.QuestionId, cancellationToken);

        if (question == null)
            return false;

        question.Status = QuestionStatus.Closed;
        question.ClosedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
