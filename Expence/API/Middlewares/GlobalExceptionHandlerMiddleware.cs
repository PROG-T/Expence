using Expence.Domain.DTOs;
using Serilog;
using System.Net;
using System.Text.Json;

namespace Expence.API.Middlewares
{
    public class GlobalExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

        public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var startTime = DateTime.UtcNow;
            var requestPath = context.Request.Path;
            var requestMethod = context.Request.Method;

            try
            {
                _logger.LogDebug("Request started. Method: {Method}, Path: {Path}",
                    requestMethod, requestPath);

                await _next(context);

                var duration = DateTime.UtcNow - startTime;
                _logger.LogInformation(
                    "Request completed successfully. Method: {Method}, Path: {Path}, StatusCode: {StatusCode}, Duration: {DurationMs}ms",
                    requestMethod, requestPath, context.Response.StatusCode, duration.TotalMilliseconds);
            }
            catch (Exception exception)
            {
                var duration = DateTime.UtcNow - startTime;
                _logger.LogError(exception,
                    "Unhandled exception in request. Method: {Method}, Path: {Path}, Duration: {DurationMs}ms",
                    requestMethod, requestPath, duration.TotalMilliseconds);

                await HandleExceptionAsync(context, exception);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var response = new BaseResponse<object>();
            var logger = context.RequestServices.GetRequiredService<ILogger<GlobalExceptionHandlerMiddleware>>();
            switch (exception)
            {
                case ArgumentNullException argNullEx:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response = new BaseResponse<object>(false, "Invalid request: required data is missing");
                    logger.LogWarning("ArgumentNullException: {Message}", argNullEx.Message);
                    break;

                case ArgumentException argEx:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response = new BaseResponse<object>(false, argEx.Message);
                    logger.LogWarning("ArgumentException: {Message}", argEx.Message);
                    break;

                case KeyNotFoundException:
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    response = new BaseResponse<object>(false, "The requested resource was not found");
                    logger.LogWarning("KeyNotFoundException: Resource not found");
                    break;

                case UnauthorizedAccessException:
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    response = new BaseResponse<object>(false, "Unauthorized access");
                    logger.LogWarning("UnauthorizedAccessException: Unauthorized access attempt");
                    break;

                case InvalidOperationException invalidOpEx:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response = new BaseResponse<object>(false, invalidOpEx.Message);
                    logger.LogWarning("InvalidOperationException: {Message}", invalidOpEx.Message);
                    break;

                case HttpRequestException httpEx:
                    context.Response.StatusCode = (int)HttpStatusCode.BadGateway;
                    response = new BaseResponse<object>(false, "External service unavailable");
                    logger.LogError("HttpRequestException: {Message}", httpEx.Message);
                    break;


                default:
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    response = new BaseResponse<object>(false, "An unexpected error occurred. Please try again later");
                    logger.LogError(exception, "Unhandled exception of type {ExceptionType}: {Message}",
                        exception.GetType().Name, exception.Message);
                    break;
            }

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var jsonResponse = JsonSerializer.Serialize(response, options);

            return context.Response.WriteAsJsonAsync(response);
        }
    }
}

