using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace CrudApp.Database;

[DebuggerDisplay("{DisplayName,nq}")]
public abstract class EntityBase
{
    protected EntityBase()
    {
        Id = EntityId.NewGuid();
        DisplayName = string.Concat(GetType().Name, "[", Id, "]");
    }

    [Key]
    public EntityId Id { get; set; }

    /// <summary>
    /// The version is used by EF Core to perform optimistic concurrency checks when updating an entity.
    /// The version is automatically incrementet in <see cref="CrudAppDbContext"/> when saving changes.
    /// </summary>
    [ConcurrencyCheck]
    public long Version { get; set; } = 1;

    public string DisplayName { get; set; }
}