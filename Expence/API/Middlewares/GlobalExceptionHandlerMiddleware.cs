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
            try
            {
                await _next(context);
            }
            catch (Exception exception)
            {
                await HandleExceptionAsync(context, exception);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var response = new BaseResponse<object>();

            switch (exception)
            {
                case ArgumentNullException:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response = new BaseResponse<object>(false, "Invalid request: required data is missing");
                    break;

                case ArgumentException argEx:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response = new BaseResponse<object>(false, argEx.Message);
                    break;

                case KeyNotFoundException:
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    response = new BaseResponse<object>(false, "The requested resource was not found");
                    break;

                case UnauthorizedAccessException:
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    response = new BaseResponse<object>(false, "Unauthorized access");
                    break;

                case InvalidOperationException invalidOpinvalEx:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response = new BaseResponse<object>(false, invalidOpinvalEx.Message);
                    break;

                default:
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    response = new BaseResponse<object>(false, "An unexpected error occurred. Please try again later");

                    // Log the full exception details for internal investigation
                    Log.Error(exception, "Unhandled exception occurred");
                    break;
            }

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var jsonResponse = JsonSerializer.Serialize(response, options);

            return context.Response.WriteAsJsonAsync(response);
        }
    }
}

