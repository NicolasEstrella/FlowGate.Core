using FlowGate.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlowGate.Core.Infrastructure.Persistence.Configurations;

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");
        builder.HasKey(auditLog => auditLog.Id);
        builder.Property(auditLog => auditLog.EntityType).HasMaxLength(128).IsRequired();
        builder.Property(auditLog => auditLog.Action).HasMaxLength(128).IsRequired();
        builder.Property(auditLog => auditLog.CorrelationId).HasMaxLength(64);
        builder.Property(auditLog => auditLog.DetailsJson).HasColumnType("jsonb");
        builder.Property(auditLog => auditLog.CreatedBy).HasMaxLength(128).IsRequired();
        builder.Property(auditLog => auditLog.ModifiedBy).HasMaxLength(128);
        builder.HasOne(auditLog => auditLog.PerformedByUser)
            .WithMany(user => user.AuditLogs)
            .HasForeignKey(auditLog => auditLog.PerformedByUserId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(auditLog => auditLog.WorkflowInstance)
            .WithMany(instance => instance.AuditLogs)
            .HasForeignKey(auditLog => auditLog.WorkflowInstanceId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}