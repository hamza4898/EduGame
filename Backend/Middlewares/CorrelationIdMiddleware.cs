using Serilog.Context;

namespace EduGame.Middlewares
{
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;
        private const string CorrelationIdHeader = "X-Correlation-ID";

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ILogger<CorrelationIdMiddleware> logger)
        {
            var correlationId = Guid.NewGuid().ToString().ToUpper();

            context.Items[CorrelationIdHeader] = correlationId;

            context.Response.Headers[CorrelationIdHeader] = correlationId;

            using (LogContext.PushProperty("CorrelationId", correlationId))
            {
                await _next(context);
            }
        }
    }
}