using FacilitationAssistant.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace FacilitationAssistant.Infrastructure.Data;

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

        modelBuilder.Entity<Meeting>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FacilitatorToken).IsRequired().HasMaxLength(50);
            entity.Property(e => e.AttendeeToken).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.HasIndex(e => e.FacilitatorToken).IsUnique();
            entity.HasIndex(e => e.AttendeeToken).IsUnique();
        });

        modelBuilder.Entity<AgendaStage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.HasOne(e => e.Meeting)
                .WithMany(m => m.Stages)
                .HasForeignKey(e => e.MeetingId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Note>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.SessionId).IsRequired().HasMaxLength(50);
            entity.HasOne(e => e.Meeting)
                .WithMany(m => m.Notes)
                .HasForeignKey(e => e.MeetingId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Stage)
                .WithMany()
                .HasForeignKey(e => e.StageId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Concern>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SessionId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ConcernType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.CustomText).HasMaxLength(500);
            entity.Property(e => e.ResponseText).HasMaxLength(500);
            entity.HasOne(e => e.Meeting)
                .WithMany(m => m.Concerns)
                .HasForeignKey(e => e.MeetingId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ConcernVote>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SessionId).IsRequired().HasMaxLength(50);
            entity.HasOne(e => e.Concern)
                .WithMany(c => c.Votes)
                .HasForeignKey(e => e.ConcernId)
                .OnDelete(DeleteBehavior.Cascade);
            // Ensure one vote per session per concern
            entity.HasIndex(e => new { e.ConcernId, e.SessionId }).IsUnique();
        });

        modelBuilder.Entity<AttendeeSession>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SessionId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.DisplayName).HasMaxLength(30);
            entity.HasOne(e => e.Meeting)
                .WithMany(m => m.AttendeeSessions)
                .HasForeignKey(e => e.MeetingId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Text).IsRequired().HasMaxLength(300);
            entity.Property(e => e.ScaleMinLabel).HasMaxLength(100);
            entity.Property(e => e.ScaleMaxLabel).HasMaxLength(100);
            entity.HasOne(e => e.Meeting)
                .WithMany(m => m.Questions)
                .HasForeignKey(e => e.MeetingId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.AssociatedStage)
                .WithMany()
                .HasForeignKey(e => e.AssociatedStageId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<QuestionOption>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OptionText).IsRequired().HasMaxLength(100);
            entity.HasOne(e => e.Question)
                .WithMany(q => q.Options)
                .HasForeignKey(e => e.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<QuestionResponse>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AttendeeSessionId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.AnswerText).HasMaxLength(1000);
            entity.HasOne(e => e.Question)
                .WithMany(q => q.Responses)
                .HasForeignKey(e => e.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Text).IsRequired().HasMaxLength(1000);
            entity.HasOne(e => e.Meeting)
                .WithMany(m => m.Messages)
                .HasForeignKey(e => e.MeetingId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MessageOption>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Text).IsRequired().HasMaxLength(100);
            entity.HasOne(e => e.Message)
                .WithMany(m => m.Options)
                .HasForeignKey(e => e.MessageId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MessageResponse>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SessionId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Reaction).HasMaxLength(10);
            entity.Property(e => e.FreeText).HasMaxLength(500);
            entity.HasOne(e => e.Message)
                .WithMany(m => m.Responses)
                .HasForeignKey(e => e.MessageId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
