using FacilitationAssistant.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace FacilitationAssistant.Infrastructure.Data;

/// <summary>
/// DbContext for Facilitation Assistant using separate Cosmos DB containers.
/// Each entity type is stored in its own container to avoid concurrency conflicts.
/// </summary>
public class FacilitationDbContext : DbContext
{
    public FacilitationDbContext(DbContextOptions<FacilitationDbContext> options)
        : base(options)
    {
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Meeting configuration
        modelBuilder.Entity<Meeting>(entity =>
        {
            entity.ToContainer("Meetings");
            entity.HasNoDiscriminator();
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FacilitatorToken).IsRequired().HasMaxLength(50);
            entity.Property(e => e.AttendeeToken).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Title).HasMaxLength(200);
            
            // Ignore navigation properties - no longer using owned entities
            entity.Ignore(m => m.Stages);
            entity.Ignore(m => m.Notes);
            entity.Ignore(m => m.Concerns);
            entity.Ignore(m => m.AttendeeSessions);
            entity.Ignore(m => m.Questions);
            entity.Ignore(m => m.Messages);
        });

        // AgendaStage configuration
        modelBuilder.Entity<AgendaStage>(entity =>
        {
            entity.ToContainer("AgendaStages");
            entity.HasNoDiscriminator();
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            
            // Index for queries
            entity.HasIndex(e => e.MeetingId);
            
            // Ignore navigation properties
            entity.Ignore(e => e.Meeting);
        });

        // Note configuration
        modelBuilder.Entity<Note>(entity =>
        {
            entity.ToContainer("Notes");
            entity.HasNoDiscriminator();
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.SessionId).IsRequired().HasMaxLength(50);
            
            // Indexes for queries
            entity.HasIndex(e => e.MeetingId);
            entity.HasIndex(e => e.SessionId);
            entity.HasIndex(e => e.StageId);
            
            // Ignore navigation properties
            entity.Ignore(e => e.Meeting);
            entity.Ignore(e => e.Stage);
        });

        // Concern configuration
        modelBuilder.Entity<Concern>(entity =>
        {
            entity.ToContainer("Concerns");
            entity.HasNoDiscriminator();
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SessionId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ConcernType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.CustomText).HasMaxLength(500);
            entity.Property(e => e.ResponseText).HasMaxLength(500);
            
            // Indexes for queries
            entity.HasIndex(e => e.MeetingId);
            entity.HasIndex(e => e.SessionId);
            
            // Ignore navigation properties
            entity.Ignore(e => e.Meeting);
            entity.Ignore(e => e.Votes);
        });

        // ConcernVote configuration
        modelBuilder.Entity<ConcernVote>(entity =>
        {
            entity.ToContainer("ConcernVotes");
            entity.HasNoDiscriminator();
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SessionId).IsRequired().HasMaxLength(50);
            
            // Indexes for queries
            entity.HasIndex(e => e.ConcernId);
            entity.HasIndex(e => e.SessionId);
            
            // Ignore navigation properties
            entity.Ignore(e => e.Concern);
        });

        // AttendeeSession configuration
        modelBuilder.Entity<AttendeeSession>(entity =>
        {
            entity.ToContainer("AttendeeSessions");
            entity.HasNoDiscriminator();
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SessionId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.DisplayName).HasMaxLength(30);
            
            // Indexes for queries
            entity.HasIndex(e => e.MeetingId);
            entity.HasIndex(e => e.SessionId);
            
            // Ignore navigation properties
            entity.Ignore(e => e.Meeting);
        });

        // Question configuration
        modelBuilder.Entity<Question>(entity =>
        {
            entity.ToContainer("Questions");
            entity.HasNoDiscriminator();
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Text).IsRequired().HasMaxLength(300);
            entity.Property(e => e.ScaleMinLabel).HasMaxLength(100);
            entity.Property(e => e.ScaleMaxLabel).HasMaxLength(100);
            
            // Indexes for queries
            entity.HasIndex(e => e.MeetingId);
            entity.HasIndex(e => e.AssociatedStageId);
            entity.HasIndex(e => e.Status);
            
            // Ignore navigation properties
            entity.Ignore(e => e.Meeting);
            entity.Ignore(e => e.AssociatedStage);
            entity.Ignore(e => e.Options);
            entity.Ignore(e => e.Responses);
        });

        // QuestionOption configuration
        modelBuilder.Entity<QuestionOption>(entity =>
        {
            entity.ToContainer("QuestionOptions");
            entity.HasNoDiscriminator();
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OptionText).IsRequired().HasMaxLength(100);
            
            // Index for queries
            entity.HasIndex(e => e.QuestionId);
            
            // Ignore navigation properties
            entity.Ignore(e => e.Question);
        });

        // QuestionResponse configuration
        modelBuilder.Entity<QuestionResponse>(entity =>
        {
            entity.ToContainer("QuestionResponses");
            entity.HasNoDiscriminator();
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AttendeeSessionId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.AnswerText).HasMaxLength(1000);
            
            // Indexes for queries
            entity.HasIndex(e => e.QuestionId);
            entity.HasIndex(e => e.AttendeeSessionId);
            
            // Ignore navigation properties
            entity.Ignore(e => e.Question);
        });

        // Message configuration
        modelBuilder.Entity<Message>(entity =>
        {
            entity.ToContainer("Messages");
            entity.HasNoDiscriminator();
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Text).IsRequired().HasMaxLength(1000);
            
            // Index for queries
            entity.HasIndex(e => e.MeetingId);
            
            // Ignore navigation properties
            entity.Ignore(e => e.Meeting);
            entity.Ignore(e => e.Options);
            entity.Ignore(e => e.Responses);
        });

        // MessageOption configuration
        modelBuilder.Entity<MessageOption>(entity =>
        {
            entity.ToContainer("MessageOptions");
            entity.HasNoDiscriminator();
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Text).IsRequired().HasMaxLength(100);
            
            // Index for queries
            entity.HasIndex(e => e.MessageId);
            
            // Ignore navigation properties
            entity.Ignore(e => e.Message);
        });

        // MessageResponse configuration
        modelBuilder.Entity<MessageResponse>(entity =>
        {
            entity.ToContainer("MessageResponses");
            entity.HasNoDiscriminator();
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SessionId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Reaction).HasMaxLength(10);
            entity.Property(e => e.FreeText).HasMaxLength(500);
            
            // Indexes for queries
            entity.HasIndex(e => e.MessageId);
            entity.HasIndex(e => e.SessionId);
            
            // Ignore navigation properties
            entity.Ignore(e => e.Message);
        });
    }
}
