using System.Diagnostics;

namespace SampleApp.WebAPI
{
    public class RequestLoggingMiddleware : IMiddleware
    {
        private readonly ILogger<RequestLoggingMiddleware> logger;

        public RequestLoggingMiddleware(ILogger<RequestLoggingMiddleware> logger)
        {
            this.logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            Activity? requestActivity = Activity.Current;

            using IDisposable? scope = logger.BeginScope(new Dictionary<string, object?>() 
            {
                ["trace.id"] = requestActivity?.TraceId.ToString(),
                ["span.id"] = requestActivity?.SpanId.ToString(),
                ["transaction.id"] = requestActivity?.SpanId.ToString(),
                ["http.request.method"] = context.Request.Method,
                ["url.full"] = $"{context.Request.Scheme}://{context.Request.Host}{context.Request.Path}{context.Request.QueryString}",
                ["url.path"] = context.Request.Path.Value,
                ["url.query"] = context.Request.QueryString.Value,
                ["event.action"] = context.GetEndpoint()?.DisplayName
            });
            
            try
            {
                await next(context);

                using var responseScope = logger.BeginScope(new Dictionary<string, object?>
                {
                    ["http.response.status_code"] = context.Response.StatusCode,
                    ["event.outcome"] = context.Response.StatusCode < 500 ? "success" : "failure"
                });

                logger.LogInformation("Request finished");
            }
            catch (Exception ex)
            {
                using var errorScope = logger.BeginScope(new Dictionary<string, object?>
                {
                    ["event.outcome"] = "failure"
                });

                logger.LogError(ex, "Unhandled exception");
                throw;
            }
        }
    }
}