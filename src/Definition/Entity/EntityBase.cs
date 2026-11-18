using Perigon.AspNetCore.Abstraction;

namespace Entity;

/// <summary>
/// 实体基类
/// </summary>
public abstract class EntityBase : IEntityBase
{
    [Key]
    public Guid Id { get; set; } = Guid.CreateVersion7();
    [AutoTimestamp]
    public DateTimeOffset CreatedTime
    {
        get; set;
    }
    [AutoTimestamp(onUpdate: true)]
    public DateTimeOffset UpdatedTime
    {
        get; set;
    }
    public bool IsDeleted
    {
        get; set;
    }
}
