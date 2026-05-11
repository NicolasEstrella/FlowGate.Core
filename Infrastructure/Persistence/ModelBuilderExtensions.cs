using FlowGate.Core.Application.Security;
using FlowGate.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FlowGate.Core.Infrastructure.Persistence;

internal static class ModelBuilderExtensions
{
    private static readonly Guid AdminRoleId = Guid.Parse("10000000-0000-0000-0000-000000000001");
    private static readonly Guid ApproverRoleId = Guid.Parse("10000000-0000-0000-0000-000000000002");
    private static readonly Guid FinanceRoleId = Guid.Parse("10000000-0000-0000-0000-000000000003");
    private static readonly Guid LegalRoleId = Guid.Parse("10000000-0000-0000-0000-000000000004");
    private static readonly Guid StandardUserRoleId = Guid.Parse("10000000-0000-0000-0000-000000000005");
    private static readonly Guid AdminUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly Guid ExpenseWorkflowId = Guid.Parse("20000000-0000-0000-0000-000000000001");

    public static void SeedFoundationData(this ModelBuilder modelBuilder)
    {
        var createdAt = new DateTimeOffset(2026, 05, 11, 0, 0, 0, TimeSpan.Zero);

        modelBuilder.Entity<ApplicationRole>().HasData(
            new ApplicationRole { Id = AdminRoleId, Name = FlowGateRoles.Admin, Description = "Administrative access", IsSystem = true, CreatedAtUtc = createdAt, CreatedBy = "seed" },
            new ApplicationRole { Id = ApproverRoleId, Name = FlowGateRoles.Approver, Description = "Approval access", IsSystem = true, CreatedAtUtc = createdAt, CreatedBy = "seed" },
            new ApplicationRole { Id = FinanceRoleId, Name = FlowGateRoles.Finance, Description = "Finance access", IsSystem = true, CreatedAtUtc = createdAt, CreatedBy = "seed" },
            new ApplicationRole { Id = LegalRoleId, Name = FlowGateRoles.Legal, Description = "Legal access", IsSystem = true, CreatedAtUtc = createdAt, CreatedBy = "seed" },
            new ApplicationRole { Id = StandardUserRoleId, Name = FlowGateRoles.StandardUser, Description = "Standard requester access", IsSystem = true, CreatedAtUtc = createdAt, CreatedBy = "seed" });

        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = AdminUserId,
                DisplayName = "Local Admin",
                Email = "admin@flowgate.local",
                ExternalIdentity = "local-admin",
                IsActive = true,
                CreatedAtUtc = createdAt,
                CreatedBy = "seed"
            });

        modelBuilder.Entity<UserRole>().HasData(
            new UserRole { UserId = AdminUserId, RoleId = AdminRoleId },
            new UserRole { UserId = AdminUserId, RoleId = ApproverRoleId });

        modelBuilder.Entity<Workflow>().HasData(
            new Workflow
            {
                Id = ExpenseWorkflowId,
                Key = "expense-reimbursement",
                Name = "Expense Reimbursement",
                Description = "Foundational workflow used to validate persistence and API contracts.",
                Version = 1,
                IsActive = true,
                CreatedAtUtc = createdAt,
                CreatedBy = "seed"
            });
    }
}