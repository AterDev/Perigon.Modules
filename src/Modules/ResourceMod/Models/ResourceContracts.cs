namespace ResourceMod.Models;

public class ResEnvironmentInput
{
    [MaxLength(60)] public required string Name { get; set; }
    [MaxLength(60)] public string? Icon { get; set; }
    [MaxLength(20)] public required string Color { get; set; }
}

public class ResCategoryInput
{
    [MaxLength(60)] public required string Name { get; set; }
    [MaxLength(60)] public required string CatalogCode { get; set; }
    [MaxLength(60)] public string? Icon { get; set; }
    [MaxLength(20)] public required string Color { get; set; }
}

public class ResGroupInput
{
    [MaxLength(60)] public required string Name { get; set; }
    [MaxLength(500)] public string? Description { get; set; }
    [MaxLength(60)] public string? Icon { get; set; }
    [MaxLength(20)] public required string Color { get; set; }
    public Guid CategoryId { get; set; }
}

public class ResTagInput
{
    [MaxLength(60)] public required string Name { get; set; }
    [MaxLength(20)] public required string Color { get; set; }
    [MaxLength(60)] public string? Icon { get; set; }
}

public class ResDefinitionInput
{
    [MaxLength(60)] public required string Name { get; set; }
    [MaxLength(60)] public string? Icon { get; set; }
    public List<ResDefinitionPropertyInput> Properties { get; set; } = [];
}

public class ResDefinitionPropertyInput
{
    public Guid? Id { get; set; }
    [MaxLength(60)] public required string Name { get; set; }
    public ResValueType ValueType { get; set; }
    public bool IsRequired { get; set; }
    [Range(1, 1000)] public int MaxLength { get; set; } = 200;
    public int Sort { get; set; }
}

public class ResourceValueInput
{
    public Guid DefinitionPropertyId { get; set; }
    [MaxLength(1000)] public required string Value { get; set; }
}

public class ResourceInput
{
    [MaxLength(120)] public required string Name { get; set; }
    [MaxLength(500)] public string? IconUrl { get; set; }
    [MaxLength(2000)] public string? Description { get; set; }
    public Guid EnvironmentId { get; set; }
    public Guid CategoryId { get; set; }
    public Guid? GroupId { get; set; }
    public Guid DefinitionId { get; set; }
    public List<string> TagNames { get; set; } = [];
    public List<ResourceValueInput> Values { get; set; } = [];
}

public class ResourceFilter : FilterBase
{
    public string? Name { get; set; }
    public Guid? EnvironmentId { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? GroupId { get; set; }
    public Guid? DefinitionId { get; set; }
    public string? TagName { get; set; }
}

public class ResPermissionInput
{
    public Guid EnvironmentId { get; set; }
    public Guid CategoryId { get; set; }
    public List<Guid> RoleIds { get; set; } = [];
}

public class ResourceItemDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? IconUrl { get; set; }
    public Guid EnvironmentId { get; set; }
    public required string EnvironmentName { get; set; }
    public Guid CategoryId { get; set; }
    public required string CategoryName { get; set; }
    public Guid? GroupId { get; set; }
    public string? GroupName { get; set; }
    public Guid DefinitionId { get; set; }
    public required string DefinitionName { get; set; }
    public List<string> TagNames { get; set; } = [];
    public DateTimeOffset UpdatedTime { get; set; }
}

public class ResourceDetailDto : ResourceItemDto
{
    public string? Description { get; set; }
    public List<ResourceValueDetailDto> Values { get; set; } = [];
}

public class ResourceValueDetailDto
{
    public Guid DefinitionPropertyId { get; set; }
    public required string Name { get; set; }
    public ResValueType ValueType { get; set; }
    public required string Value { get; set; }
}

public class ResourceSelectionDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Icon { get; set; }
    public string? Color { get; set; }
}
