namespace EntityFramework.EntityConfigration.SystemMod;

public sealed class SysDataScopeGroupConfiguration : IEntityTypeConfiguration<SysDataScopeGroup>
{
    public void Configure(EntityTypeBuilder<SysDataScopeGroup> builder)
    {
        builder.Property(group => group.IsEnabled)
            .HasDefaultValue(true);
    }
}