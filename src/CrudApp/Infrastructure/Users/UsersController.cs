using Microsoft.AspNetCore.Mvc;

namespace CrudApp.Infrastructure.Users;

public class UsersController : EntityControllerBase<User>
{
    [HttpGet("current")]
    public async Task<User?> GetCurrentUser()
    {
        var userId = AuthenticationContext.Current?.UserId;
        if (!userId.HasValue)
            return default;

        var user = await DbContext.GetByIdAuthorized<User>(userId.Value, asNoTracking: true);
        return user;
    }
}
