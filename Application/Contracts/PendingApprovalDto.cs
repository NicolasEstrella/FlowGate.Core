namespace FlowGate.Core.Application.Contracts;

public sealed record PendingApprovalDto(
    Guid WorkflowInstanceId,
    string Title,
    string WorkflowName,
    string RequesterName,
    Guid StepId,
    string StepName,
    string ExecutionType,
    string? GroupId,
    DateTimeOffset CreatedAt);
