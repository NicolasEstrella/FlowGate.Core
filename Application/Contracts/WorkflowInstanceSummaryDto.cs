namespace FlowGate.Core.Application.Contracts;

public sealed record WorkflowInstanceSummaryDto(
    Guid Id,
    Guid WorkflowId,
    string WorkflowName,
    string Title,
    string CurrentStatus,
    Guid RequesterId,
    string RequesterName,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? SubmittedAtUtc,
    DateTimeOffset? CompletedAtUtc);