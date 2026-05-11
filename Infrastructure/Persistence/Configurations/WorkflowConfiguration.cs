using FlowGate.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlowGate.Core.Infrastructure.Persistence.Configurations;

public sealed class WorkflowConfiguration : IEntityTypeConfiguration<Workflow>
{
    public void Configure(EntityTypeBuilder<Workflow> builder)
    {
        builder.ToTable("workflows");
        builder.HasKey(workflow => workflow.Id);
        builder.Property(workflow => workflow.Key).HasMaxLength(100).IsRequired();
        builder.Property(workflow => workflow.Name).HasMaxLength(200).IsRequired();
        builder.Property(workflow => workflow.Description).HasMaxLength(1000);
        builder.Property(workflow => workflow.CreatedBy).HasMaxLength(128).IsRequired();
        builder.Property(workflow => workflow.ModifiedBy).HasMaxLength(128);
        builder.HasIndex(workflow => new { workflow.Key, workflow.Version }).IsUnique();
    }
}