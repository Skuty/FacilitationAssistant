using FacilitationAssistant.Core.Domain.Aggregates;
using FacilitationAssistant.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FacilitationAssistant.Infrastructure.Persistence;

/// <summary>
/// Application database context.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Meeting> Meetings => Set<Meeting>();
    public DbSet<AgendaStage> AgendaStages => Set<AgendaStage>();
    public DbSet<Attendee> Attendees => Set<Attendee>();
    public DbSet<Poll> Polls => Set<Poll>();
    public DbSet<PollOption> PollOptions => Set<PollOption>();
    public DbSet<Vote> Votes => Set<Vote>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<Concern> Concerns => Set<Concern>();
    public DbSet<Note> Notes => Set<Note>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
