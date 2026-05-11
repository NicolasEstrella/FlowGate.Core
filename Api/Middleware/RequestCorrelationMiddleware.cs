namespace FlowGate.Core.Api.Middleware;

public sealed class RequestCorrelationMiddleware(RequestDelegate next, ILogger<RequestCorrelationMiddleware> logger)
{
    public const string CorrelationIdHeaderName = "X-Correlation-Id";

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers[CorrelationIdHeaderName].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(correlationId))
        {
            correlationId = Guid.NewGuid().ToString("N");
        }

        context.Items[CorrelationIdHeaderName] = correlationId;
        context.Response.Headers[CorrelationIdHeaderName] = correlationId;
        context.TraceIdentifier = correlationId;

        using (logger.BeginScope(new Dictionary<string, object?>
        {
            [CorrelationIdHeaderName] = correlationId
        }))
        {
            await next(context);
        }
    }
}