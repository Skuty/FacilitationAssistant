using FacilitationAssistant.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FacilitationAssistant.Infrastructure.Persistence.Configurations;

public class AgendaStageConfiguration : IEntityTypeConfiguration<AgendaStage>
{
    public void Configure(EntityTypeBuilder<AgendaStage> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.Description)
            .HasMaxLength(2000);

        builder.Property(s => s.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Ignore(s => s.Notes);
    }
}
