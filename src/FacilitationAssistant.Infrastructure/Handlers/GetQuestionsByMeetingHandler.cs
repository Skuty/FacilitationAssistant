using FacilitationAssistant.Core.Entities;
using FacilitationAssistant.Core.Queries;
using FacilitationAssistant.Infrastructure.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FacilitationAssistant.Infrastructure.Handlers;

public class GetQuestionsByMeetingHandler : IRequestHandler<GetQuestionsByMeetingQuery, List<Question>>
{
    private readonly FacilitationDbContext _context;

    public GetQuestionsByMeetingHandler(FacilitationDbContext context)
    {
        _context = context;
    }

    public async ValueTask<List<Question>> Handle(GetQuestionsByMeetingQuery request, CancellationToken cancellationToken)
    {
        return await _context.Questions
            .Where(q => q.MeetingId == request.MeetingId)
            .OrderBy(q => q.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
