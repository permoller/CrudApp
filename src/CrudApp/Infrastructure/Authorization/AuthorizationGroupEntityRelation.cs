using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrudApp.Infrastructure.Authorization;

/// <summary>
/// Defines that an entity is included in a group.
/// </summary>
[EntityTypeConfiguration(typeof(Configuration))]
public class AuthorizationGroupEntityRelation : EntityBase
{
    public EntityId AuthorizationGroupId { get; set; }
    public EntityId EntityId { get; set; }
    public string EntityType { get; set; } = null!;

    public sealed class Configuration : IEntityTypeConfiguration<AuthorizationGroupEntityRelation>
    {
        public void Configure(EntityTypeBuilder<AuthorizationGroupEntityRelation> builder)
        {
            builder.HasOne<AuthorizationGroup>().WithMany().HasForeignKey(e => e.AuthorizationGroupId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}

