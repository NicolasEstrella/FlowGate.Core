namespace FlowGate.Core.Application.Contracts;

public sealed record HealthStatusDto(
    string Status,
    string Environment,
    DateTimeOffset TimestampUtc,
    bool DatabaseReady,
    string CorrelationId);