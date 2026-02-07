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
    public DbSet<AttendeeSession> AttendeeSessions { get; set; }

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
            entity.HasOne(e => e.Meeting)
                .WithMany(m => m.Concerns)
                .HasForeignKey(e => e.MeetingId)
                .OnDelete(DeleteBehavior.Cascade);
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
    }
}
