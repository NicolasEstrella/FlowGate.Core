using FlowGate.Core.Domain.Common;

namespace FlowGate.Core.Domain.Entities;

public sealed class ApplicationRole : AuditableEntity
{
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public bool IsSystem { get; set; } = true;

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}