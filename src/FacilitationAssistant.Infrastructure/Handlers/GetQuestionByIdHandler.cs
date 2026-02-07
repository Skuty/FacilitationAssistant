using FacilitationAssistant.Core.Entities;
using FacilitationAssistant.Core.Queries;
using FacilitationAssistant.Infrastructure.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FacilitationAssistant.Infrastructure.Handlers;

public class GetQuestionByIdHandler : IRequestHandler<GetQuestionByIdQuery, Question?>
{
    private readonly FacilitationDbContext _context;

    public GetQuestionByIdHandler(FacilitationDbContext context)
    {
        _context = context;
    }

    public async ValueTask<Question?> Handle(GetQuestionByIdQuery request, CancellationToken cancellationToken)
    {
        return await _context.Questions
            .Include(q => q.Options)
            .Include(q => q.Responses)
            .FirstOrDefaultAsync(q => q.Id == request.QuestionId, cancellationToken);
    }
}
