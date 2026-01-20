using FacilitationAssistant.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FacilitationAssistant.Infrastructure.Persistence.Configurations;

public class ConcernConfiguration : IEntityTypeConfiguration<Concern>
{
    public void Configure(EntityTypeBuilder<Concern> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Content)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(c => c.RaisedBySessionId)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.RaisedByName)
            .HasMaxLength(100);

        builder.Property(c => c.Severity)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(c => c.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(c => c.FacilitatorResponse)
            .HasMaxLength(2000);

        builder.HasIndex(c => c.Status);
    }
}
