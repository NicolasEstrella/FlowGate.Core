using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace FlowGate.Core.Infrastructure.Persistence;

public static class DatabaseInitializationExtensions
{
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<FlowGateDbContext>();

        var pending = await dbContext.Database.GetPendingMigrationsAsync();
        if (!pending.Any()) return;

        try
        {
            await dbContext.Database.MigrateAsync();
        }
        catch (PostgresException ex) when (ex.SqlState == "42P07")
        {
            // Migrations history table already exists — database was previously initialized
        }
    }
}