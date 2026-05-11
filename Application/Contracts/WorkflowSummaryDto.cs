namespace FlowGate.Core.Application.Contracts;

public sealed record WorkflowSummaryDto(
    Guid Id,
    string Key,
    string Name,
    int Version,
    bool IsActive,
    int StepCount);