namespace CSVService.API.Middleware;

using Microsoft.AspNetCore.Http;

/// <summary>
/// Middleware that inspects the "Accept" HTTP header in incoming requests.
/// If the "Accept" header specifies media types that are incompatible with JSON responses,
/// the middleware responds with a 406 Not Acceptable status code.
/// </summary>
internal class NotAcceptableMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue("Accept", out var acceptValues))
        {
            await next(context);
            return;
        }

        string accept = acceptValues.ToString();
        if (string.IsNullOrWhiteSpace(accept) || IsJsonAccepted(accept))
        {
            await next(context);
            return;
        }

        if (!context.Response.HasStarted)
        {
            await Results.Problem(
                    title: "Not Acceptable",
                    detail: "The requested media types are not compatible with JSON responses.",
                    statusCode: StatusCodes.Status406NotAcceptable)
                .ExecuteAsync(context);
        }
    }

    private static bool IsJsonAccepted(string acceptHeader)
    {
        // Split by comma for multiple media ranges
        string[] parts = acceptHeader.Split(',');
        foreach (string part in parts)
        {
            string item = part.Trim();
            if (item.Length == 0)
                continue;

            // Strip any parameters (e.g., ;q=0.8)
            int semicolonIndex = item.IndexOf(';');
            string mediaType = semicolonIndex >= 0 ? item[..semicolonIndex].Trim() : item;
            mediaType = mediaType.ToLowerInvariant();

            // Wildcards that include JSON
            if (mediaType == "*/*")
                return true;

            // application/* covers application/json
            if (mediaType == "application/*")
                return true;

            // Exact JSON type
            if (mediaType == "application/json")
                return true;

            // Any vendor types that end with +json (e.g., application/vnd.api+json)
            if (mediaType.EndsWith("+json"))
                return true;
        }

        return false;
    }
}