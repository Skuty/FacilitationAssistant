using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FacilitationAssistant.Infrastructure.Data;

/// <summary>
/// Initializes Cosmos DB database and containers with proper partition keys.
/// EF Core's EnsureCreatedAsync() doesn't reliably create all containers for Cosmos DB.
/// </summary>
public class CosmosDbInitializer
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<CosmosDbInitializer> _logger;

    public CosmosDbInitializer(IConfiguration configuration, ILogger<CosmosDbInitializer> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Ensures database and all containers exist with proper partition keys.
    /// </summary>
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var endpoint = _configuration.GetValue<string>("Database:ConnectionStrings:CosmosDB:AccountEndpoint");
        var key = _configuration.GetValue<string>("Database:ConnectionStrings:CosmosDB:AccountKey");
        var databaseName = _configuration.GetValue<string>("Database:ConnectionStrings:CosmosDB:DatabaseName") ?? "FacilitationAssistant";

        if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(key))
        {
            _logger.LogWarning("Cosmos DB credentials not configured, skipping initialization");
            return;
        }

        using var cosmosClient = new CosmosClient(endpoint, key);

        // Create database if not exists
        var databaseResponse = await cosmosClient.CreateDatabaseIfNotExistsAsync(
            databaseName,
            ThroughputProperties.CreateAutoscaleThroughput(1000),
            cancellationToken: cancellationToken);

        _logger.LogInformation("Database {DatabaseName} ready (created: {Created})", 
            databaseName, databaseResponse.StatusCode == System.Net.HttpStatusCode.Created);

        var database = databaseResponse.Database;

        // Define all containers with their partition keys
        var containers = new[]
        {
            new { Name = "Meetings", PartitionKey = "/Id" },
            new { Name = "AgendaStages", PartitionKey = "/MeetingId" },
            new { Name = "Notes", PartitionKey = "/MeetingId" },
            new { Name = "Concerns", PartitionKey = "/MeetingId" },
            new { Name = "ConcernVotes", PartitionKey = "/ConcernId" },
            new { Name = "AttendeeSessions", PartitionKey = "/MeetingId" },
            new { Name = "Questions", PartitionKey = "/MeetingId" },
            new { Name = "QuestionOptions", PartitionKey = "/QuestionId" },
            new { Name = "QuestionResponses", PartitionKey = "/QuestionId" },
            new { Name = "Messages", PartitionKey = "/MeetingId" },
            new { Name = "MessageOptions", PartitionKey = "/MessageId" },
            new { Name = "MessageResponses", PartitionKey = "/MessageId" }
        };

        foreach (var container in containers)
        {
            var containerResponse = await database.CreateContainerIfNotExistsAsync(
                container.Name,
                container.PartitionKey,
                throughput: 400, // Minimum throughput per container
                cancellationToken: cancellationToken);

            _logger.LogInformation("Container {ContainerName} ready (partition: {PartitionKey}, created: {Created})",
                container.Name, container.PartitionKey, containerResponse.StatusCode == System.Net.HttpStatusCode.Created);
        }

        _logger.LogInformation("Cosmos DB initialization complete");
    }
}
