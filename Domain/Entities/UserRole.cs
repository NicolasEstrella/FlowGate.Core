namespace FlowGate.Core.Domain.Entities;

public sealed class UserRole
{
    public Guid UserId { get; set; }

    public User User { get; set; } = null!;

    public Guid RoleId { get; set; }

    public ApplicationRole Role { get; set; } = null!;
}