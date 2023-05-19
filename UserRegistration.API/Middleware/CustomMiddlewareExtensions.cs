using Microsoft.AspNetCore.Builder;

using UserRegistrationAPI.API.DataContracts.Settings;

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
