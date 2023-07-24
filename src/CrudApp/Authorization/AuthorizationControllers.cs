using CrudApp.Controllers;

namespace CrudApp.Authorization;

[Tags("Authorization")]
public sealed class AuthorizationGroupController : EntityControllerBase<AuthorizationGroup> { }

[Tags("Authorization")]
public sealed class AuthorizationGroupEntityController : EntityControllerBase<AuthorizationGroupEntity> { }

[Tags("Authorization")]
public sealed class AuthorizationGroupUserController : EntityControllerBase<AuthorizationGroupMembership> { }

[Tags("Authorization")]
public sealed class AuthorizationRoleController : EntityControllerBase<AuthorizationRole> { }