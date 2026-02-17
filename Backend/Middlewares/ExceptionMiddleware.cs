using System.Net;
using System.Text.Json;
using EduGame.Exceptions;

namespace EduGame.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (AuthException ex)
            {
                await HandleExceptionAsync(context, ex.Message, HttpStatusCode.BadRequest);
            }
            catch (Exception)
            {
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
