using FacilitationAssistant.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace FacilitationAssistant.Infrastructure.Data.Cosmos;

/// <summary>
/// Cosmos DB-specific EF Core model configuration.
/// Stores each entity type in its own container to avoid concurrency conflicts.
/// Navigation properties are ignored because Cosmos does not support joins across containers.
/// </summary>
public static class CosmosModelConfiguration
{
    public static void Apply(ModelBuilder modelBuilder)
    {
        // Meeting configuration
        modelBuilder.Entity<Meeting>(entity =>
        {
            entity.ToContainer("Meetings");
            entity.HasNoDiscriminator();
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FacilitatorToken).IsRequired().HasMaxLength(50);
            entity.Property(e => e.AttendeeToken).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Title).HasMaxLength(200);

            // Ignore navigation properties - no joins across containers in Cosmos
            entity.Ignore(m => m.Stages);
            entity.Ignore(m => m.Notes);
            entity.Ignore(m => m.Concerns);
            entity.Ignore(m => m.AttendeeSessions);
            entity.Ignore(m => m.Questions);
            entity.Ignore(m => m.Messages);
            entity.Ignore(m => m.StageProposals);
        });

        // AgendaStage configuration
        modelBuilder.Entity<AgendaStage>(entity =>
        {
            entity.ToContainer("AgendaStages");
            entity.HasNoDiscriminator();
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);

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

            entity.Ignore(e => e.Question);
        });

        // Message configuration
        modelBuilder.Entity<Message>(entity =>
        {
            entity.ToContainer("Messages");
            entity.HasNoDiscriminator();
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Text).IsRequired().HasMaxLength(1000);

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

            entity.Ignore(e => e.Message);
        });

        // StageProposal configuration
        modelBuilder.Entity<StageProposal>(entity =>
        {
            entity.ToContainer("StageProposals");
            entity.HasNoDiscriminator();
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SessionId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ProposerName).HasMaxLength(50);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.RejectionReason).HasMaxLength(500);

            entity.Ignore(e => e.Meeting);
        });
    }
}
