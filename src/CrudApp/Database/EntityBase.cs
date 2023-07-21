using CrudApp.ChangeTracking;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CrudApp.Database;

public abstract class EntityBase
{
    protected EntityBase()
    {
        Id = EntityId.NewGuid();
    }

    /// <summary>
    /// The Id uniquely identifies this enitity among all entities. Not just among enitities of the same type.
    /// </summary>
    // The id is used as the primary key in the database on all entitties.
    [Key]
    // By default EF Core assumes the id is database-generated. But we generate it in the constructor.
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public EntityId Id { get; set; }

    /// <summary>
    /// The version is used by EF Core to perform optimistic concurrency checks when updating an entity.
    /// The version is automatically incrementet in <see cref="CrudAppDbContext"/> when saving changes.
    /// </summary>
    [ConcurrencyCheck]
    public long Version { get; set; } = 1;

    /// <summary>
    /// Flag indicating the entity is deleted and should normally not be included in the results when querying for entities.
    /// </summary>
    public bool IsSoftDeleted { get; set; }

    /// <summary>
    /// String that within "some context" allows a human user to easaly recognize/identify
    /// an instance of an entity among other entities of the same type.
    /// </summary>
    [NotMapped]
    public virtual string DisplayName => Id.ToString();

    public ICollection<EntityChangeEvent> EntityChangeEvents { get; set; } = new List<EntityChangeEvent>();

    public override string ToString()
    {
        return DisplayName;
    }
}