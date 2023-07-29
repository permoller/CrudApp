using CrudApp.Infrastructure.Authentication;
using CrudApp.Infrastructure.Users;

namespace CrudApp.Infrastructure.Authorization;

public static class AuthorizationApplicationBuilderExtensions
{
    public static T UseCrudAppAuthorizationContext<T>(this T app) where T : IApplicationBuilder
    {
        app.Use(async (ctx, next) => {

            if (long.TryParse(ctx.User.Claims.FirstOrDefault(c => c.Type == UserIdAuthenticationHandler.UserIdClaimType)?.Value, out var userId))
                AuthorizationContext.Current = new(new User { Id = userId });
            try
            {
                await next();
            }
            finally
            {
                AuthorizationContext.Current = null;
            }
        });
        
        return app;
    }
}