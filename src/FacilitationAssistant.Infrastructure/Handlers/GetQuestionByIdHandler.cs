using FacilitationAssistant.Core.Entities;
using FacilitationAssistant.Core.Queries;
using FacilitationAssistant.Infrastructure.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FacilitationAssistant.Infrastructure.Handlers;

/// <summary>
/// Handles retrieving a question by its ID.
/// </summary>
public class GetQuestionByIdHandler : IRequestHandler<GetQuestionByIdQuery, Question?>
{
    private readonly IDbContextFactory<FacilitationDbContext> _contextFactory;

    public GetQuestionByIdHandler(IDbContextFactory<FacilitationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async ValueTask<Question?> Handle(GetQuestionByIdQuery request, CancellationToken cancellationToken)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        
        return await context.Questions
            .FirstOrDefaultAsync(q => q.Id == request.QuestionId, cancellationToken);
    }
}
