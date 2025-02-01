using System.Diagnostics;

namespace GrandChessTree.Api.Middleware
{
    public class RequestTimingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestTimingMiddleware> _logger;

        public RequestTimingMiddleware(RequestDelegate next, ILogger<RequestTimingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                await _next(context); // Process request
            }
            finally
            {
                stopwatch.Stop();
                _logger.LogInformation("Request {Method} {Path} completed in {ElapsedMilliseconds} ms",
                    context.Request.Method, context.Request.Path, stopwatch.ElapsedMilliseconds);
            }
        }
    }

}
