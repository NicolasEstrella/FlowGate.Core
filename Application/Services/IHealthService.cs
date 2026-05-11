using FlowGate.Core.Application.Contracts;

namespace FlowGate.Core.Application.Services;

public interface IHealthService
{
    Task<HealthStatusDto> GetStatusAsync(string correlationId, CancellationToken cancellationToken);
}