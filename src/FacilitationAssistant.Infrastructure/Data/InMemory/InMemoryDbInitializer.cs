using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FacilitationAssistant.Infrastructure.Data.InMemory;

/// <summary>
/// Database initializer for the InMemory provider.
/// Calls EnsureCreated so the in-memory store is ready before the first request.
/// </summary>
public class InMemoryDbInitializer : IDatabaseInitializer
{
    private readonly IDbContextFactory<FacilitationDbContext> _contextFactory;
    private readonly ILogger<InMemoryDbInitializer> _logger;

    public InMemoryDbInitializer(
        IDbContextFactory<FacilitationDbContext> contextFactory,
        ILogger<InMemoryDbInitializer> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        await context.Database.EnsureCreatedAsync(cancellationToken);
        _logger.LogInformation("InMemory database initialized");
    }
}
