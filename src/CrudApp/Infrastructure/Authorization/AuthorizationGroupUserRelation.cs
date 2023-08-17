using CrudApp.Infrastructure.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrudApp.Infrastructure.Authorization;

/// <summary>
/// Defines that a user has the rights defined by a role on the entities in a group.
/// </summary>
// A user can not have the same role multiple times in the same group.
[Index(nameof(UserId), nameof(AuthorizationGroupId), nameof(AuthorizationRoleId), IsUnique = true)]
public class AuthorizationGroupUserRelation : EntityBase
{
    public EntityId AuthorizationGroupId { get; set; }
    public EntityId UserId { get; set; }
    public EntityId AuthorizationRoleId { get; set; }

    public AuthorizationGroup? AuthorizationGroup { get; set; }
    public AuthorizationRole? AuthorizationRole { get; set; }
    public sealed class Configuration : IEntityTypeConfiguration<AuthorizationGroupUserRelation>
    {
        public void Configure(EntityTypeBuilder<AuthorizationGroupUserRelation> builder)
        {
            builder.HasOne<User>().WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
