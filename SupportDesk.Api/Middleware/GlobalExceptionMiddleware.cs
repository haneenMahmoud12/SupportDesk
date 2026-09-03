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
            catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
            {
                logger.LogDebug(
                    "Request was cancelled by the client. TraceId: {TraceId}",
                    context.TraceIdentifier);
            }
            catch (Exception exception)
            {
                if (context.Response.HasStarted)
                {
                    logger.LogError(
                        exception,
                        "An exception occurred after the response started. TraceId: {TraceId}",
                        context.TraceIdentifier);
                    throw;
                }

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

                SupportDesk.Application.Exceptions.BadRequestException =>
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
