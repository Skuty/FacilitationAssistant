using FacilitationAssistant.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FacilitationAssistant.Infrastructure.Persistence.Configurations;

public class VoteConfiguration : IEntityTypeConfiguration<Vote>
{
    public void Configure(EntityTypeBuilder<Vote> builder)
    {
        builder.HasKey(v => v.Id);

        builder.Property(v => v.VoterSessionId)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(v => new { v.VoterSessionId, v.OptionId });
    }
}
