namespace FlowGate.Core.Application.Engine;

public sealed record CreateRequestCommand(
    string WorkflowKey,
    string Title,
    IReadOnlyDictionary<string, string> FormData,
    Guid RequesterId);

public sealed record SubmitDecisionCommand(
    Guid WorkflowInstanceId,
    Guid StepId,
    DecisionType Decision,
    string PerformedBy,
    string? Comment);

public enum DecisionType
{
    Approve,
    Reject,
    RequestAdjustment
}

public interface IWorkflowEngine
{
    Task<Guid> StartAsync(CreateRequestCommand command, CancellationToken cancellationToken = default);
    Task SubmitAsync(Guid instanceId, string performedBy, CancellationToken cancellationToken = default);
    Task ProcessDecisionAsync(SubmitDecisionCommand command, CancellationToken cancellationToken = default);
    Task CancelAsync(Guid instanceId, string performedBy, CancellationToken cancellationToken = default);
}
