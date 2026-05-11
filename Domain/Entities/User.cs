using FlowGate.Core.Domain.Common;

namespace FlowGate.Core.Domain.Entities;

public sealed class User : AuditableEntity
{
    public string DisplayName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? ExternalIdentity { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    public ICollection<WorkflowInstance> RequestedWorkflowInstances { get; set; } = new List<WorkflowInstance>();

    public ICollection<ApprovalStep> AssignedApprovalSteps { get; set; } = new List<ApprovalStep>();

    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}