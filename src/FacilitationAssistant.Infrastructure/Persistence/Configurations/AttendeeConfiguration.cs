using FacilitationAssistant.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FacilitationAssistant.Infrastructure.Persistence.Configurations;

public class AttendeeConfiguration : IEntityTypeConfiguration<Attendee>
{
    public void Configure(EntityTypeBuilder<Attendee> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.SessionId)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.DisplayName)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(a => a.SessionId);
    }
}
