using Microsoft.AspNetCore.Diagnostics;

namespace ts89070.Extensions;

internal static class ProblemDetailsExtensions
{
    public static void AddProblemDetails(this IServiceCollection services)
    {
        services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = ctx =>
            {
                var traceId = ctx.HttpContext?.TraceIdentifier;
                if (!string.IsNullOrWhiteSpace(traceId))
                {
                    ctx.ProblemDetails.Extensions["traceId"] = traceId;
                }
            };
        });
    }

    public static void UseProblemDetails(this IApplicationBuilder app)
    {
        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                var feature = context.Features.Get<IExceptionHandlerFeature>();
                var ex = feature?.Error;

                var pds = context.RequestServices.GetRequiredService<IProblemDetailsService>();

                if (ex is BadHttpRequestException)
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await pds.WriteAsync(new ProblemDetailsContext
                    {
                        HttpContext = context,
                        ProblemDetails =
                        {
                            Title = "Invalid JSON in request body",
                            Detail = "The request body could not be parsed as the expected JSON object.",
                            Status = StatusCodes.Status400BadRequest,
                            Type = "https://httpstatuses.com/400"
                        }
                    });
                    return;
                }

                // Fallback: 500
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await pds.WriteAsync(new ProblemDetailsContext
                {
                    HttpContext = context,
                    ProblemDetails =
                    {
                        Title = "Unexpected error",
                        Status = StatusCodes.Status500InternalServerError,
                        Type = "https://httpstatuses.com/500"
                    }
                });
            });
        });
    }
}
