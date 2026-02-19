using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FacilitationAssistant.Infrastructure.Data.SqlServer;

/// <summary>
/// Database initializer for SQL Server.
/// Applies any pending EF Core migrations on startup.
/// </summary>
public class SqlServerDbInitializer : IDatabaseInitializer
{
    private readonly IDbContextFactory<FacilitationDbContext> _contextFactory;
    private readonly ILogger<SqlServerDbInitializer> _logger;

    public SqlServerDbInitializer(
        IDbContextFactory<FacilitationDbContext> contextFactory,
        ILogger<SqlServerDbInitializer> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Applying SQL Server migrations...");

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        await context.Database.MigrateAsync(cancellationToken);

        _logger.LogInformation("SQL Server migrations applied successfully");
    }
}
