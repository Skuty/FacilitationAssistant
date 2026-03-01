using FacilitationAssistant.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace FacilitationAssistant.Infrastructure.Data.SqlServer;

/// <summary>
/// SQL Server (and InMemory) relational model configuration.
/// Defines proper tables, foreign keys, indexes, and cascade rules.
/// </summary>
public static class SqlServerModelConfiguration
{
    public static void Apply(ModelBuilder modelBuilder)
    {
        // ── Meeting ───────────────────────────────────────────────────────────
        modelBuilder.Entity<Meeting>(entity =>
        {
            entity.ToTable("Meetings");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.FacilitatorToken).IsRequired().HasMaxLength(50);
            entity.Property(e => e.AttendeeToken).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Title).HasMaxLength(200);

            entity.HasIndex(e => e.FacilitatorToken).IsUnique();
            entity.HasIndex(e => e.AttendeeToken).IsUnique();

            entity.HasMany(m => m.Stages)
                  .WithOne(s => s.Meeting)
                  .HasForeignKey(s => s.MeetingId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(m => m.Notes)
                  .WithOne(n => n.Meeting)
                  .HasForeignKey(n => n.MeetingId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(m => m.Concerns)
                  .WithOne(c => c.Meeting)
                  .HasForeignKey(c => c.MeetingId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(m => m.AttendeeSessions)
                  .WithOne(a => a.Meeting)
                  .HasForeignKey(a => a.MeetingId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(m => m.Questions)
                  .WithOne(q => q.Meeting)
                  .HasForeignKey(q => q.MeetingId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(m => m.Messages)
                  .WithOne(msg => msg.Meeting)
                  .HasForeignKey(msg => msg.MeetingId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ── AgendaStage ───────────────────────────────────────────────────────
        modelBuilder.Entity<AgendaStage>(entity =>
        {
            entity.ToTable("AgendaStages");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);

            entity.HasIndex(e => new { e.MeetingId, e.OrderIndex });
        });

        // ── Note ──────────────────────────────────────────────────────────────
        modelBuilder.Entity<Note>(entity =>
        {
            entity.ToTable("Notes");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.SessionId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.AuthorName).HasMaxLength(50);

            entity.HasIndex(e => e.MeetingId);

            // Optional relationship to AgendaStage — set null when stage is deleted.
            // ClientSetNull (not SetNull) avoids the SQL Server "multiple cascade paths" error:
            // Meeting → Notes (CASCADE) and Meeting → Stages → Notes (CASCADE + SetNull) would
            // both touch Notes. EF Core handles the null-out in memory instead of at DB level.
            entity.HasOne(n => n.Stage)
                  .WithMany()
                  .HasForeignKey(n => n.StageId)
                  .IsRequired(false)
                  .OnDelete(DeleteBehavior.ClientSetNull);
        });

        // ── Concern ───────────────────────────────────────────────────────────
        modelBuilder.Entity<Concern>(entity =>
        {
            entity.ToTable("Concerns");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.SessionId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ConcernType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.CustomText).HasMaxLength(500);
            entity.Property(e => e.ResponseText).HasMaxLength(500);

            entity.HasIndex(e => e.MeetingId);

            entity.HasMany(c => c.Votes)
                  .WithOne(v => v.Concern)
                  .HasForeignKey(v => v.ConcernId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ── ConcernVote ───────────────────────────────────────────────────────
        modelBuilder.Entity<ConcernVote>(entity =>
        {
            entity.ToTable("ConcernVotes");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.SessionId).IsRequired().HasMaxLength(50);

            entity.HasIndex(e => e.ConcernId);
        });

        // ── AttendeeSession ───────────────────────────────────────────────────
        modelBuilder.Entity<AttendeeSession>(entity =>
        {
            entity.ToTable("AttendeeSessions");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.SessionId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.DisplayName).HasMaxLength(30);

            entity.HasIndex(e => e.MeetingId);
            entity.HasIndex(e => new { e.MeetingId, e.SessionId });
        });

        // ── Question ──────────────────────────────────────────────────────────
        modelBuilder.Entity<Question>(entity =>
        {
            entity.ToTable("Questions");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Text).IsRequired().HasMaxLength(300);
            entity.Property(e => e.ScaleMinLabel).HasMaxLength(100);
            entity.Property(e => e.ScaleMaxLabel).HasMaxLength(100);

            entity.HasIndex(e => e.MeetingId);

            // Optional relationship to AssociatedStage — set null when stage is deleted.
            // ClientSetNull (not SetNull) avoids the SQL Server "multiple cascade paths" error:
            // Meeting → Questions (CASCADE) and Meeting → Stages → Questions (CASCADE + SetNull)
            // would both touch Questions. EF Core handles the null-out in memory instead.
            entity.HasOne(q => q.AssociatedStage)
                  .WithMany()
                  .HasForeignKey(q => q.AssociatedStageId)
                  .IsRequired(false)
                  .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasMany(q => q.Options)
                  .WithOne(o => o.Question)
                  .HasForeignKey(o => o.QuestionId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(q => q.Responses)
                  .WithOne(r => r.Question)
                  .HasForeignKey(r => r.QuestionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ── QuestionOption ────────────────────────────────────────────────────
        modelBuilder.Entity<QuestionOption>(entity =>
        {
            entity.ToTable("QuestionOptions");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.OptionText).IsRequired().HasMaxLength(100);

            entity.HasIndex(e => new { e.QuestionId, e.OrderIndex });
        });

        // ── QuestionResponse ──────────────────────────────────────────────────
        modelBuilder.Entity<QuestionResponse>(entity =>
        {
            entity.ToTable("QuestionResponses");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.AttendeeSessionId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.AuthorName).HasMaxLength(50);
            entity.Property(e => e.AnswerText).HasMaxLength(1000);

            entity.HasIndex(e => e.QuestionId);
            entity.HasIndex(e => new { e.QuestionId, e.AttendeeSessionId });

            // EF Core 9 primitive collection — stored as JSON column in SQL Server
            entity.Property(e => e.AnswerChoiceIds)
                  .HasColumnType("nvarchar(max)");
        });

        // ── Message ───────────────────────────────────────────────────────────
        modelBuilder.Entity<Message>(entity =>
        {
            entity.ToTable("Messages");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Text).IsRequired().HasMaxLength(1000);

            entity.HasIndex(e => e.MeetingId);

            entity.HasMany(m => m.Options)
                  .WithOne(o => o.Message)
                  .HasForeignKey(o => o.MessageId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(m => m.Responses)
                  .WithOne(r => r.Message)
                  .HasForeignKey(r => r.MessageId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ── StageProposal ─────────────────────────────────────────────────────
        modelBuilder.Entity<StageProposal>(entity =>
        {
            entity.ToTable("StageProposals");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.SessionId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ProposerName).HasMaxLength(50);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.RejectionReason).HasMaxLength(500);

            entity.HasIndex(e => e.MeetingId);
            entity.HasIndex(e => new { e.MeetingId, e.Status });

            // Back-reference to the Meeting using ClientSetNull to avoid cascade conflicts
            entity.HasOne(p => p.Meeting)
                  .WithMany(m => m.StageProposals)
                  .HasForeignKey(p => p.MeetingId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ── MessageOption ─────────────────────────────────────────────────────
        modelBuilder.Entity<MessageOption>(entity =>
        {
            entity.ToTable("MessageOptions");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Text).IsRequired().HasMaxLength(100);

            entity.HasIndex(e => new { e.MessageId, e.OrderIndex });
        });

        // ── MessageResponse ───────────────────────────────────────────────────
        modelBuilder.Entity<MessageResponse>(entity =>
        {
            entity.ToTable("MessageResponses");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.SessionId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Reaction).HasMaxLength(10);
            entity.Property(e => e.FreeText).HasMaxLength(500);

            entity.HasIndex(e => e.MessageId);
            entity.HasIndex(e => new { e.MessageId, e.SessionId });
        });
    }
}
