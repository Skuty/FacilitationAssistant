using FacilitationAssistant.Infrastructure.Data;
using FacilitationAssistant.Infrastructure.Data.Cosmos;
using FacilitationAssistant.Infrastructure.Data.InMemory;
using FacilitationAssistant.Infrastructure.Data.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FacilitationAssistant.Infrastructure.Extensions;

public static class DatabaseServiceExtensions
{
    /// <summary>
    /// Registers the correct EF Core provider and <see cref="IDatabaseInitializer"/> based on
    /// the <c>Database:Provider</c> configuration value ("InMemory", "CosmosDb", or "SqlServer").
    /// </summary>
    public static IServiceCollection AddDatabase(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var providerName = configuration.GetValue<string>("Database:Provider") ?? "InMemory";

        if (!Enum.TryParse<DatabaseProvider>(providerName, ignoreCase: true, out var provider))
        {
            throw new InvalidOperationException(
                $"Unknown database provider '{providerName}'. " +
                $"Valid values are: {string.Join(", ", Enum.GetNames<DatabaseProvider>())}");
        }

        // Register provider enum as singleton so FacilitationDbContext can read it via DI
        services.AddSingleton(typeof(DatabaseProvider), (object)provider);

        switch (provider)
        {
            case DatabaseProvider.CosmosDb:
            {
                var endpoint = configuration.GetValue<string>("Database:ConnectionStrings:CosmosDb:AccountEndpoint")
                    ?? throw new InvalidOperationException(
                        "Database:ConnectionStrings:CosmosDb:AccountEndpoint is required when using the CosmosDb provider.");

                var key = configuration.GetValue<string>("Database:ConnectionStrings:CosmosDb:AccountKey")
                    ?? throw new InvalidOperationException(
                        "Database:ConnectionStrings:CosmosDb:AccountKey is required when using the CosmosDb provider.");

                var dbName = configuration.GetValue<string>("Database:ConnectionStrings:CosmosDb:DatabaseName")
                    ?? "FacilitationAssistant";

                services.AddDbContextFactory<FacilitationDbContext>(options =>
                    options.UseCosmos(endpoint, key, dbName));

                services.AddSingleton<IDatabaseInitializer, CosmosDbInitializer>();

                LogProviderSelection(services, DatabaseProvider.CosmosDb, dbName);
                break;
            }

            case DatabaseProvider.SqlServer:
            {
                var connectionString = configuration.GetValue<string>("Database:ConnectionStrings:SqlServer")
                    ?? throw new InvalidOperationException(
                        "Database:ConnectionStrings:SqlServer is required when using the SqlServer provider.");

                services.AddDbContextFactory<FacilitationDbContext>(options =>
                    options.UseSqlServer(connectionString, sqlOptions =>
                    {
                        sqlOptions.MigrationsAssembly(
                            typeof(DatabaseServiceExtensions).Assembly.GetName().Name);
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorNumbersToAdd: null);
                    }));

                services.AddSingleton<IDatabaseInitializer, SqlServerDbInitializer>();

                LogProviderSelection(services, DatabaseProvider.SqlServer, connectionString);
                break;
            }

            case DatabaseProvider.InMemory:
            default:
            {
                var dbName = configuration.GetValue<string>("Database:ConnectionStrings:InMemory")
                    ?? "FacilitationDb";

                services.AddDbContextFactory<FacilitationDbContext>(options =>
                    options.UseInMemoryDatabase(dbName));

                services.AddSingleton<IDatabaseInitializer, InMemoryDbInitializer>();

                LogProviderSelection(services, DatabaseProvider.InMemory, dbName);
                break;
            }
        }

        return services;
    }

    private static void LogProviderSelection(IServiceCollection services, DatabaseProvider provider, string target)
    {
        // Use a build-time service provider just for the logger, to emit startup info
        // (actual runtime logging happens in Program.cs after Build())
        Console.WriteLine($"[Database] Provider: {provider} | Target: {target}");
    }
}
