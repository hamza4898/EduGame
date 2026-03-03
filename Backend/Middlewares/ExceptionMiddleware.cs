using System.Net;
using System.Text.Json;

namespace EduGame.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError("Server Error: {ex}, More info: {RequestDetails}", ex, new
                {
                    Path = context.Request.Path.ToString(),
                    Method = context.Request.Method,
                    Query = context.Request.Query.ToString()
                });

                await HandleExceptionAsync(context, "Внутренняя ошибка сервера", HttpStatusCode.InternalServerError);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, string message, HttpStatusCode statusCode)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var responce = new { errors = new[] { message } };

            return context.Response.WriteAsJsonAsync(responce);
        }
    }
    
}
