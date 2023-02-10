using Microsoft.AspNetCore.Builder;
using UserRegistration.API.Common.Settings;

namespace UserRegistration.API.Middleware
{
    public static class CustomMiddlewareExtensions
    {
        public static void ConfigureCustomMiddleware(this IApplicationBuilder app, AppSettings? settings = default)
        {
            app.UseMiddleware<ExceptionMiddleware>(settings);
        }
    }
}
