using FlowGate.Core.Domain.Common;

namespace FlowGate.Core.Domain.Entities;

public sealed class Workflow : AuditableEntity
{
    public string Key { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int Version { get; set; } = 1;

    public bool IsActive { get; set; } = true;

    public ICollection<WorkflowInstance> Instances { get; set; } = new List<WorkflowInstance>();
}