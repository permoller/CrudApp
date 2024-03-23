using Microsoft.AspNetCore.Mvc;

namespace CrudApp.Infrastructure.Users;

public class UsersController : EntityControllerBase<User>
{
    [HttpGet("current")]
    public async Task<Result<Maybe<User>>> GetCurrentUser(CancellationToken cancellationToken)
    {
        var userId = AuthenticationContext.Current?.UserId;
        if (!userId.HasValue)
            return Maybe<User>.NoValue;

        var userResult = await DbContext.GetByIdAuthorized<User>(userId.Value, asNoTracking: true, cancellationToken)
            .Validate(user => user.IsSoftDeleted ? new Error.CannotGetDeletedEntity(typeof(User), userId.Value) : null)
            .Map(user => Maybe.From(user));
        return userResult;
    }
}
