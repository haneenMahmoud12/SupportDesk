using SupportDesk.Application.Models;

namespace SupportDesk.Api.Middleware
{
    public sealed class GlobalExceptionMiddleware(
    RequestDelegate next,
    ILogger<GlobalExceptionMiddleware> logger)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception exception)
            {
                await HandleExceptionAsync(context, exception);
            }
        }

        private async Task HandleExceptionAsync(
            HttpContext context,
            Exception exception)
        {
            var (statusCode, message) = exception switch
            {
                KeyNotFoundException =>
                    (StatusCodes.Status404NotFound, exception.Message),

                UnauthorizedAccessException =>
                    (StatusCodes.Status403Forbidden, exception.Message),

                ArgumentException =>
                    (StatusCodes.Status400BadRequest, exception.Message),

                _ =>
                    (StatusCodes.Status500InternalServerError,
                     "An unexpected error occurred.")
            };

            if (statusCode >= 500)
            {
                logger.LogError(
                    exception,
                    "Unhandled exception. TraceId: {TraceId}",
                    context.TraceIdentifier);
            }
            else
            {
                logger.LogWarning(
                    exception,
                    "Request failed with status {StatusCode}. TraceId: {TraceId}",
                    statusCode,
                    context.TraceIdentifier);
            }

            if (context.Response.HasStarted)
            {
                throw exception;
            }

            context.Response.Clear();
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsJsonAsync(new ResponseModel
            {
                Succeeded = false,
                Errors = [message]
            });
        }
    }
}
