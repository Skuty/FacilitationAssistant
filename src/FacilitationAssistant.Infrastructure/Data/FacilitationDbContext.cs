using FacilitationAssistant.Core.Entities;
using FacilitationAssistant.Infrastructure.Data.Cosmos;
using FacilitationAssistant.Infrastructure.Data.SqlServer;
using Microsoft.EntityFrameworkCore;

namespace FacilitationAssistant.Infrastructure.Data;

/// <summary>
/// EF Core DbContext for Facilitation Assistant.
/// Applies model configuration conditionally based on the active <see cref="DatabaseProvider"/>.
/// Supports Cosmos DB, SQL Server, and InMemory providers.
/// </summary>
public class FacilitationDbContext : DbContext
{
    private readonly DatabaseProvider _provider;

    public FacilitationDbContext(
        DbContextOptions<FacilitationDbContext> options,
        DatabaseProvider provider = DatabaseProvider.InMemory)
        : base(options)
    {
        _provider = provider;
    }

    public DbSet<Meeting> Meetings { get; set; }
    public DbSet<AgendaStage> AgendaStages { get; set; }
    public DbSet<Note> Notes { get; set; }
    public DbSet<Concern> Concerns { get; set; }
    public DbSet<ConcernVote> ConcernVotes { get; set; }
    public DbSet<AttendeeSession> AttendeeSessions { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<QuestionOption> QuestionOptions { get; set; }
    public DbSet<QuestionResponse> QuestionResponses { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<MessageOption> MessageOptions { get; set; }
    public DbSet<MessageResponse> MessageResponses { get; set; }
    public DbSet<StageProposal> StageProposals { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        switch (_provider)
        {
            case DatabaseProvider.CosmosDb:
                CosmosModelConfiguration.Apply(modelBuilder);
                break;
            case DatabaseProvider.SqlServer:
                SqlServerModelConfiguration.Apply(modelBuilder);
                break;
            case DatabaseProvider.InMemory:
            default:
                // InMemory reuses relational config; provider ignores unsupported hints
                SqlServerModelConfiguration.Apply(modelBuilder);
                break;
        }
    }

}

