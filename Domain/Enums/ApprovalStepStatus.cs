namespace FlowGate.Core.Domain.Enums;

public enum ApprovalStepStatus
{
    Pending = 0,
    InProgress = 1,
    Approved = 2,
    Rejected = 3,
    AdjustmentsRequested = 4,
    Cancelled = 5
}