namespace FlowGate.Core.Application.Contracts;

public sealed record CreateWorkflowInstanceRequest(
    string WorkflowKey,
    string Title,
    IReadOnlyDictionary<string, string> FormData);

public sealed record SubmitDecisionRequest(
    Guid StepId,
    string Decision,
    string? Comment);
