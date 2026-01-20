using FacilitationAssistant.Core.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FacilitationAssistant.Infrastructure.Persistence.Configurations;

public class MeetingConfiguration : IEntityTypeConfiguration<Meeting>
{
    public void Configure(EntityTypeBuilder<Meeting> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(m => m.FacilitatorKey)
            .IsRequired();

        builder.Property(m => m.State)
            .IsRequired()
            .HasConversion<string>();

        builder.HasMany(m => m.Stages)
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(m => m.Attendees)
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(m => m.Polls)
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(m => m.Messages)
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(m => m.Concerns)
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(m => m.Notes)
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(m => m.FacilitatorKey);
    }
}
