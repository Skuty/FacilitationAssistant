using FacilitationAssistant.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FacilitationAssistant.Infrastructure.Persistence.Configurations;

public class NoteConfiguration : IEntityTypeConfiguration<Note>
{
    public void Configure(EntityTypeBuilder<Note> builder)
    {
        builder.HasKey(n => n.Id);

        builder.Property(n => n.Content)
            .IsRequired()
            .HasMaxLength(5000);

        builder.Property(n => n.OwnerId)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(n => n.OwnerName)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(n => new { n.OwnerId, n.IsPrivate });
        builder.HasIndex(n => n.LinkedStageId);
    }
}
