using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace FacilitationAssistant.Infrastructure.Data;

/// <summary>
/// Design-time factory for creating DbContext instances during migrations.
/// This is used by EF Core tools to create the DbContext for migration commands.
/// </summary>
public class FacilitationDbContextFactory : IDesignTimeDbContextFactory<FacilitationDbContext>
{
    public FacilitationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<FacilitationDbContext>();
        
        // Use a default connection string for migrations
        // This can be overridden by the actual runtime configuration
        var connectionString = "Host=localhost;Port=5432;Database=facilitation_assistant;Username=facilitation_user;Password=facilitation_pass";
        
        optionsBuilder.UseNpgsql(connectionString);
        
        return new FacilitationDbContext(optionsBuilder.Options);
    }
}
