using FlowGate.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FlowGate.Core.Infrastructure.Persistence;

public sealed class FlowGateDbContext(DbContextOptions<FlowGateDbContext> options) : DbContext(options)
{
    public DbSet<ApplicationRole> Roles => Set<ApplicationRole>();

    public DbSet<User> Users => Set<User>();

    public DbSet<UserRole> UserRoles => Set<UserRole>();

    public DbSet<Workflow> Workflows => Set<Workflow>();

    public DbSet<WorkflowInstance> WorkflowInstances => Set<WorkflowInstance>();

    public DbSet<ApprovalStep> ApprovalSteps => Set<ApprovalStep>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FlowGateDbContext).Assembly);
        modelBuilder.SeedFoundationData();
        base.OnModelCreating(modelBuilder);
    }
}