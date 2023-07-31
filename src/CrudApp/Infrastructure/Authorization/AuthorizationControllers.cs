namespace CrudApp.Infrastructure.Authorization;

[Tags("Authorization")]
public sealed class AuthorizationGroupController : EntityControllerBase<AuthorizationGroup> { }

[Tags("Authorization")]
public sealed class AuthorizationGroupEntityController : EntityControllerBase<AuthorizationGroupEntityRelation> { }

[Tags("Authorization")]
public sealed class AuthorizationGroupUserController : EntityControllerBase<AuthorizationGroupUserRelation> { }

[Tags("Authorization")]
public sealed class AuthorizationRoleController : EntityControllerBase<AuthorizationRole> { }