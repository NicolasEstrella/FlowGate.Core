using FlowGate.Core.Domain.Common;
using FlowGate.Core.Domain.Enums;

namespace FlowGate.Core.Domain.Entities;

public sealed class WorkflowInstance : AuditableEntity
{
    public Guid WorkflowId { get; set; }

    public Workflow Workflow { get; set; } = null!;

    public Guid RequesterId { get; set; }

    public User Requester { get; set; } = null!;

    public string Title { get; set; } = string.Empty;

    public WorkflowInstanceStatus CurrentStatus { get; set; } = WorkflowInstanceStatus.Draft;

    public DateTimeOffset? SubmittedAtUtc { get; set; }

    public DateTimeOffset? CompletedAtUtc { get; set; }

    public ICollection<ApprovalStep> ApprovalSteps { get; set; } = new List<ApprovalStep>();

    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}