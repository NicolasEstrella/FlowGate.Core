namespace FlowGate.Core.Domain.Enums;

public enum WorkflowInstanceStatus
{
    Draft = 0,
    Submitted = 1,
    InApproval = 2,
    Approved = 3,
    Rejected = 4,
    AdjustmentsRequested = 5,
    Cancelled = 6
}