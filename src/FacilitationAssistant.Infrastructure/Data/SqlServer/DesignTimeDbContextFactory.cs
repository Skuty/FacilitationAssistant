using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FacilitationAssistant.Infrastructure.Data.SqlServer;

/// <summary>
/// Design-time factory used by the EF Core CLI tools (dotnet ef migrations add / update)
/// without requiring the full application host to start.
/// 
/// Usage:
///   dotnet ef migrations add InitialCreate \
///     --project src/FacilitationAssistant.Infrastructure \
///     --startup-project src/FacilitationAssistant.Web \
///     --context FacilitationDbContext \
///     --output-dir Data/SqlServer/Migrations
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<FacilitationDbContext>
{
    public FacilitationDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<FacilitationDbContext>()
            .UseSqlServer(
                "Server=(localdb)\\mssqllocaldb;Database=FacilitationAssistant_Design;Trusted_Connection=True;",
                sqlOptions => sqlOptions.MigrationsAssembly(
                    typeof(DesignTimeDbContextFactory).Assembly.GetName().Name))
            .Options;

        return new FacilitationDbContext(options, DatabaseProvider.SqlServer);
    }
}
