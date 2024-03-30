using Microsoft.AspNetCore.Mvc;

namespace CrudApp.Infrastructure.Users;

public class UsersController : EntityControllerBase<User>
{
    [HttpGet("current")]
    public Task<Maybe<Result<User>>> GetCurrentUser(CancellationToken cancellationToken)
    {
        return AuthenticationContext.Current.ToMaybe()
            .Select(ctx => DbContext.GetByIdAuthorized<User>(ctx.UserId, asNoTracking: true, cancellationToken));
    }
}
