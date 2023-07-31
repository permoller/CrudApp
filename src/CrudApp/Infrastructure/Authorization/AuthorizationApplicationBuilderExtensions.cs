using CrudApp.Infrastructure.Authentication;

namespace CrudApp.Infrastructure.Authorization;

public static class AuthenticationApplicationBuilderExtensions
{
    public static T UseCrudAppAuthorizationContext<T>(this T app) where T : IApplicationBuilder
    {
        app.Use(async (ctx, next) => {

            if (AuthenticationContext.Current is not null)
                AuthorizationContext.Current = new(AuthenticationContext.Current.User);
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