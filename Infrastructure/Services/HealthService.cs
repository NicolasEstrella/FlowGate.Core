using FlowGate.Core.Application.Contracts;
using FlowGate.Core.Application.Services;
using FlowGate.Core.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace FlowGate.Core.Infrastructure.Services;

public sealed class HealthService(FlowGateDbContext dbContext, IHostEnvironment hostEnvironment) : IHealthService
{
    public async Task<HealthStatusDto> GetStatusAsync(string correlationId, CancellationToken cancellationToken)
    {
        var databaseReady = await dbContext.Database.CanConnectAsync(cancellationToken);

        return new HealthStatusDto(
            databaseReady ? "Healthy" : "Degraded",
            hostEnvironment.EnvironmentName,
            DateTimeOffset.UtcNow,
            databaseReady,
            correlationId);
    }
}