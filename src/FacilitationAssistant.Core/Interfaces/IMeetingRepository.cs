using FacilitationAssistant.Core.Domain.Aggregates;

namespace FacilitationAssistant.Core.Interfaces;

/// <summary>
/// Repository for Meeting aggregate operations.
/// </summary>
public interface IMeetingRepository
{
    Task<Meeting?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Meeting?> GetByFacilitatorKeyAsync(Guid facilitatorKey, CancellationToken ct = default);
    Task AddAsync(Meeting meeting, CancellationToken ct = default);
    Task UpdateAsync(Meeting meeting, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
}
