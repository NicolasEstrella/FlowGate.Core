using FlowGate.Core.Domain.Common;

namespace FlowGate.Core.Domain.Entities;

public sealed class AuditLog : AuditableEntity
{
    public string EntityType { get; set; } = string.Empty;

    public Guid EntityId { get; set; }

    public string Action { get; set; } = string.Empty;

    public Guid? PerformedByUserId { get; set; }

    public User? PerformedByUser { get; set; }

    public Guid? WorkflowInstanceId { get; set; }

    public WorkflowInstance? WorkflowInstance { get; set; }

    public string? CorrelationId { get; set; }

    public string? DetailsJson { get; set; }
}