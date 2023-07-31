namespace CrudApp.Infrastructure.Authorization;

public static class AuthorizationApplicationBuilderExtensions
{
    public static T UseCrudAppAuthenticationContext<T>(this T app) where T : IApplicationBuilder
    {
        app.Use(async (ctx, next) => {

            if (long.TryParse(ctx.User.Claims.FirstOrDefault(c => c.Type == UserIdAuthenticationHandler.UserIdClaimType)?.Value, out var userId))
                AuthenticationContext.Current = new(userId);
            try
            {
                await next();
            }
            finally
            {
                AuthenticationContext.Current = null;
            }
        });
        
        return app;
    }
}