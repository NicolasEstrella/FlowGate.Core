using Microsoft.EntityFrameworkCore;

namespace FlowGate.Core.Infrastructure.Persistence;

public static class DatabaseInitializationExtensions
{
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<FlowGateDbContext>();
        await dbContext.Database.MigrateAsync();
    }
}