using FlowGate.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlowGate.Core.Infrastructure.Persistence.Configurations;

public sealed class ApprovalStepConfiguration : IEntityTypeConfiguration<ApprovalStep>
{
    public void Configure(EntityTypeBuilder<ApprovalStep> builder)
    {
        builder.ToTable("approval_steps");
        builder.HasKey(step => step.Id);
        builder.Property(step => step.Name).HasMaxLength(200).IsRequired();
        builder.Property(step => step.RequiredRole).HasMaxLength(64).IsRequired();
        builder.Property(step => step.Status).HasConversion<string>().HasMaxLength(64).IsRequired();
        builder.Property(step => step.DecisionComment).HasMaxLength(1000);
        builder.Property(step => step.CreatedBy).HasMaxLength(128).IsRequired();
        builder.Property(step => step.ModifiedBy).HasMaxLength(128);
        builder.HasOne(step => step.WorkflowInstance)
            .WithMany(instance => instance.ApprovalSteps)
            .HasForeignKey(step => step.WorkflowInstanceId);
        builder.HasOne(step => step.AssignedUser)
            .WithMany(user => user.AssignedApprovalSteps)
            .HasForeignKey(step => step.AssignedUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}