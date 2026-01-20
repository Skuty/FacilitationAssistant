using FacilitationAssistant.Core.Domain.Aggregates;
using FacilitationAssistant.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FacilitationAssistant.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Meeting aggregate.
/// </summary>
public class MeetingRepository : IMeetingRepository
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public MeetingRepository(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<Meeting?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        
        return await context.Meetings
            .Include(m => m.Stages)
            .Include(m => m.Attendees)
            .Include(m => m.Polls)
                .ThenInclude(p => p.Options)
            .Include(m => m.Polls)
                .ThenInclude(p => p.Votes)
            .Include(m => m.Messages)
            .Include(m => m.Concerns)
            .Include(m => m.Notes)
            .AsSplitQuery()
            .FirstOrDefaultAsync(m => m.Id == id, ct);
    }

    public async Task<Meeting?> GetByFacilitatorKeyAsync(Guid facilitatorKey, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        
        return await context.Meetings
            .Include(m => m.Stages)
            .Include(m => m.Attendees)
            .Include(m => m.Polls)
                .ThenInclude(p => p.Options)
            .Include(m => m.Polls)
                .ThenInclude(p => p.Votes)
            .Include(m => m.Messages)
            .Include(m => m.Concerns)
            .Include(m => m.Notes)
            .AsSplitQuery()
            .FirstOrDefaultAsync(m => m.FacilitatorKey == facilitatorKey, ct);
    }

    public async Task AddAsync(Meeting meeting, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        
        await context.Meetings.AddAsync(meeting, ct);
        await context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Meeting meeting, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        
        context.Meetings.Update(meeting);
        await context.SaveChangesAsync(ct);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        
        return await context.Meetings.AnyAsync(m => m.Id == id, ct);
    }
}
