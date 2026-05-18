using FlowGate.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlowGate.Core.Infrastructure.Persistence.Configurations;

public sealed class WorkflowInstanceConfiguration : IEntityTypeConfiguration<WorkflowInstance>
{
    public void Configure(EntityTypeBuilder<WorkflowInstance> builder)
    {
        builder.ToTable("workflow_instances");
        builder.HasKey(instance => instance.Id);
        builder.Property(instance => instance.Title).HasMaxLength(200).IsRequired();
        builder.Property(instance => instance.CreatedBy).HasMaxLength(128).IsRequired();
        builder.Property(instance => instance.ModifiedBy).HasMaxLength(128);
        builder.Property(instance => instance.CurrentStatus).HasConversion<string>().HasMaxLength(64).IsRequired();
        builder.Property(instance => instance.FormDataJson).HasColumnType("jsonb");
        builder.HasOne(instance => instance.Workflow)
            .WithMany(workflow => workflow.Instances)
            .HasForeignKey(instance => instance.WorkflowId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(instance => instance.Requester)
            .WithMany(user => user.RequestedWorkflowInstances)
            .HasForeignKey(instance => instance.RequesterId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}