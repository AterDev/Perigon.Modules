namespace EntityFramework.EntityConfigration.ResourceMod;

public sealed class UserResourceConfiguration : IEntityTypeConfiguration<UserResource>
{
    public void Configure(EntityTypeBuilder<UserResource> builder)
    {
        builder.HasOne(resource => resource.Definition)
            .WithMany()
            .HasForeignKey(resource => resource.DefinitionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}