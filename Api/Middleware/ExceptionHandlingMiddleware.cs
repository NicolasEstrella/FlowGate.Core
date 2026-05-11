using Microsoft.AspNetCore.Mvc;

namespace FlowGate.Core.Api.Middleware;

public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unhandled exception while processing request {Path}", context.Request.Path);

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/problem+json";

            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An unexpected error occurred.",
                Detail = "The request could not be processed. Use the correlation identifier for support.",
                Instance = context.Request.Path
            };

            problemDetails.Extensions["correlationId"] = context.TraceIdentifier;

            await context.Response.WriteAsJsonAsync(problemDetails);
        }
    }
}