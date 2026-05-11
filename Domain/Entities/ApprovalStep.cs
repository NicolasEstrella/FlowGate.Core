using FlowGate.Core.Domain.Common;
using FlowGate.Core.Domain.Enums;

namespace FlowGate.Core.Domain.Entities;

public sealed class ApprovalStep : AuditableEntity
{
    public Guid WorkflowInstanceId { get; set; }

    public WorkflowInstance WorkflowInstance { get; set; } = null!;

    public int Order { get; set; }

    public string Name { get; set; } = string.Empty;

    public string RequiredRole { get; set; } = string.Empty;

    public ApprovalStepStatus Status { get; set; } = ApprovalStepStatus.Pending;

    public Guid? AssignedUserId { get; set; }

    public User? AssignedUser { get; set; }

    public DateTimeOffset? DueAtUtc { get; set; }

    public DateTimeOffset? DecidedAtUtc { get; set; }

    public string? DecisionComment { get; set; }
}