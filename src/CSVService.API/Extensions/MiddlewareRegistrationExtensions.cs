using CSVService.API.Middleware;

namespace CSVService.API.Extensions;

/// <summary>
/// Provides extension methods for registering middleware components into the application's request pipeline.
/// </summary>
internal static class MiddlewareRegistrationExtensions
{
    /// <summary>
    /// Registers the NotAcceptableMiddleware into the application's request pipeline to handle unsupported "Accept" headers.
    /// This middleware inspects the "Accept" HTTP header in incoming requests. If the "Accept" header specifies media types
    /// that are incompatible with JSON responses, it generates a response with a 406 Not Acceptable status code.
    /// </summary>
    /// <param name="app">The application pipeline to which the middleware is added.</param>
    /// <returns>The same <see cref="IApplicationBuilder"/> instance provided, to allow for chaining further middleware registration.</returns>
    public static IApplicationBuilder UseNotAcceptableForUnsupportedAccept(this IApplicationBuilder app)
    {
        return app.UseMiddleware<NotAcceptableMiddleware>();
    }
}