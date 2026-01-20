using FacilitationAssistant.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FacilitationAssistant.Infrastructure.Persistence.Configurations;

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Content)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(m => m.SenderSessionId)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(m => m.SenderName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(m => m.Type)
            .IsRequired()
            .HasConversion<string>();

        builder.HasIndex(m => m.Timestamp);
    }
}
