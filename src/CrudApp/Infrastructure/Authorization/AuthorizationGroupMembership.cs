﻿using CrudApp.Infrastructure.Users;
using Microsoft.EntityFrameworkCore;

namespace CrudApp.Infrastructure.Authorization;

/// <summary>
/// Defines that a user has the rights defined by a role on the entities in a group.
/// </summary>
// A user can not have the same role multiple times in the same group.
[Index(nameof(UserId), nameof(AuthorizationGroupId), nameof(AuthorizationRoleId), IsUnique = true)]
public class AuthorizationGroupMembership : EntityBase
{
    public EntityId AuthorizationGroupId { get; set; }
    public EntityId UserId { get; set; }
    public EntityId AuthorizationRoleId { get; set; }

    public AuthorizationGroup AuthorizationGroup { get; set; } = null!;
    public User User { get; set; } = null!;
    public AuthorizationRole AuthorizationRole { get; set; } = null!;
}