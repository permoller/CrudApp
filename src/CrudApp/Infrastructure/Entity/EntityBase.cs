﻿using CrudApp.Infrastructure.ChangeTracking;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CrudApp.Infrastructure.Entity;

public abstract class EntityBase
{
    public static Func<EntityId> NewEntityId { get; set; } =
        () => throw new InvalidOperationException(
            $"{nameof(EntityBase)}.{nameof(NewEntityId)} needs to be initialized before it is used.");

    // We need a lock to make sure we do not accidently generate multiple different IDs for the same entity.
    // It could be an instance specific object (not static) and that might mean less blocking of threads.
    // But that would also mean we get an extra object for each entity-instance, which requires more memory and garbage collection.
    private static readonly object _idLock = new();

    
    private EntityId? _id;

    protected EntityBase() { }

    /// <summary>
    /// <see cref="Id"/> uniquely identifies this enitity among all entities. Not just among enitities of the same type.
    /// It is NOT generated by the database. If it has not been set, a new value is generated the first time the property is read.
    /// It is used as the primary key in the database.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public EntityId Id
    {
        get
        {
            if (_id == default)
            {
                lock (_idLock)
                {
                    if (_id == default)
                    {
                        _id = NewEntityId();
                    }
                }
            }
            return _id!.Value;
        }

        set
        {
            lock (_idLock)
            {
                _id = value;
            }
        }
    }

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
    /// String that within "some context" allows a human user to easily recognize/identify
    /// an instance of an entity among other entities of the same type.
    /// </summary>
    [NotMapped]
    public virtual string DisplayName => GetType().Name + Id.ToString();

    public ICollection<EntityChange> EntityChanges { get; set; } = new List<EntityChange>();

    public override string ToString()
    {
        return DisplayName;
    }

    public static void ConfigureEntityModel(EntityTypeBuilder entityTypeBuilder)
    {
        if (!entityTypeBuilder.Metadata.ClrType.IsSubclassOf(typeof(EntityBase)))
            throw new ArgumentException($"{entityTypeBuilder.Metadata.ClrType.Name} is not a subtype of {nameof(EntityBase)}.");

        // Force EF Core to use the Id property and not the backing field to ensure ID is generated on new entities.
        entityTypeBuilder.Property(nameof(Id)).UsePropertyAccessMode(PropertyAccessMode.Property);
    }
}