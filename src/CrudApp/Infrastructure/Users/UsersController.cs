using Microsoft.AspNetCore.Mvc;

namespace CrudApp.Infrastructure.Users;

public class UsersController : EntityControllerBase<User>
{
    [HttpGet("current")]
    public async Task<Result<Maybe<User>>> GetCurrentUser(CancellationToken cancellationToken)
    {
        var userId = AuthenticationContext.Current?.UserId;
        if (!userId.HasValue)
            return Maybe.NoValue<User>();

        var userResult = await DbContext.GetByIdAuthorized<User>(userId.Value, asNoTracking: true, cancellationToken)
            .Select(user => user.ToMaybe());
        return userResult;
    }
}
