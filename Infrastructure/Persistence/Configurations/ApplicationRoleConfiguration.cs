using FlowGate.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlowGate.Core.Infrastructure.Persistence.Configurations;

public sealed class ApplicationRoleConfiguration : IEntityTypeConfiguration<ApplicationRole>
{
    public void Configure(EntityTypeBuilder<ApplicationRole> builder)
    {
        builder.ToTable("application_roles");
        builder.HasKey(role => role.Id);
        builder.Property(role => role.Name).HasMaxLength(64).IsRequired();
        builder.Property(role => role.Description).HasMaxLength(256).IsRequired();
        builder.Property(role => role.CreatedBy).HasMaxLength(128).IsRequired();
        builder.Property(role => role.ModifiedBy).HasMaxLength(128);
        builder.HasIndex(role => role.Name).IsUnique();
    }
}