namespace EntityFramework.EntityConfigration.ResourceMod;

public sealed class UserResValueConfiguration : IEntityTypeConfiguration<UserResValue>
{
    public void Configure(EntityTypeBuilder<UserResValue> builder)
    {
        builder.HasOne(value => value.DefinitionProperty)
            .WithMany()
            .HasForeignKey(value => value.DefinitionPropertyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}