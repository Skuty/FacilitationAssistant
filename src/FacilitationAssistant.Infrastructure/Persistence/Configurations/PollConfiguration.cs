using FacilitationAssistant.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FacilitationAssistant.Infrastructure.Persistence.Configurations;

public class PollConfiguration : IEntityTypeConfiguration<Poll>
{
    public void Configure(EntityTypeBuilder<Poll> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Question)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(p => p.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.HasMany(p => p.Options)
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Votes)
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade);
    }
}
