using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace FlowGate.Core.Infrastructure.Persistence;

public sealed class FlowGateDbContextFactory : IDesignTimeDbContextFactory<FlowGateDbContext>
{
    public FlowGateDbContext CreateDbContext(string[] args)
    {
        var basePath = Directory.GetCurrentDirectory();

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

        var optionsBuilder = new DbContextOptionsBuilder<FlowGateDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new FlowGateDbContext(optionsBuilder.Options);
    }
}